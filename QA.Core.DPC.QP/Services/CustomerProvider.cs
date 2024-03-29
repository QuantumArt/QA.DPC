﻿using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using Npgsql;
using QA.Core.DPC.QP.Exceptions;
using QA.Core.DPC.QP.Models;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.QP.Services
{
    public class CustomerProvider : ICustomerProvider
    {
        private const int Timeout = 2;
        private readonly NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IntegrationProperties _integrationProps;

        public CustomerProvider(IOptions<IntegrationProperties> integrationProps)
        {
            _integrationProps = integrationProps.Value;
        }

        public string GetConnectionString(string customerCode)
        {
            DBConnector.ConfigServiceUrl = _integrationProps.ConfigurationServiceUrl;
            DBConnector.ConfigServiceToken = _integrationProps.ConfigurationServiceToken;
            return DBConnector.GetConnectionString(customerCode);
        }

        public Customer GetCustomer(string customerCode)
        {
            try
            {
                DBConnector.ConfigServiceUrl = _integrationProps.ConfigurationServiceUrl;
                DBConnector.ConfigServiceToken = _integrationProps.ConfigurationServiceToken;
                var configuration = DBConnector.GetQpConfiguration().Result;
                var customerConfiguration = configuration.Customers.FirstOrDefault(c => c.Name == customerCode);
                if (customerConfiguration == null)
                {
                    if (customerCode == SingleCustomerCoreProvider.Key)
                    {
                        return new Customer() { CustomerCode = SingleCustomerCoreProvider.Key };
                    }
                    throw new ConsolidationException($"Customer code '{customerCode}' not found");
                }
                return new Customer
                {
                    ConnectionString = customerConfiguration.ConnectionString.Replace("Provider=SQLOLEDB;", ""),
                    CustomerCode = customerConfiguration.Name,
                    DatabaseType = customerConfiguration.DbType
                };
            }
            catch (Exception ex)
            {
                throw new ConsolidationException($"Customer code '{customerCode}' not found", ex);
            }
        }

        public Customer[] GetCustomers(bool onlyConsolidated = true)
        {
            var result = new Customer[] { };
            DBConnector.ConfigServiceUrl = _integrationProps.ConfigurationServiceUrl;
            DBConnector.ConfigServiceToken = _integrationProps.ConfigurationServiceToken;
            _logger.Info(() => $"Config service url :{DBConnector.ConfigServiceUrl}");
            var configuration = DBConnector.GetQpConfiguration().Result;
            var customers = configuration.Customers;
            _logger.Info(() => $"Received customers: {customers.Length}");
            if (customers != null)
            {
                result =
                    customers.Select(c => new Customer
                        {
                            ConnectionString = c.ConnectionString.Replace("Provider=SQLOLEDB;", ""),
                            CustomerCode = c.Name,
                            DatabaseType = c.DbType
                        })
                        .Select(UpdateIsConsolidated)
                        .Where(c => !onlyConsolidated || c.IsConsolidated)
                        .ToArray();
            }
            
            _logger.Info(() => $"Customers after filtering: {result.Length}");
            return result;
        }

        private Customer UpdateIsConsolidated(Customer customer)
        {
            try
            {
                var builder = customer.DatabaseType == DatabaseType.SqlServer
                    ? (DbConnectionStringBuilder) new SqlConnectionStringBuilder(customer.ConnectionString)
                        {ConnectTimeout = Timeout}
                    : new NpgsqlConnectionStringBuilder(customer.ConnectionString) {CommandTimeout = Timeout};
                var connector = new DBConnector(builder.ConnectionString, customer.DatabaseType);
                var command =  connector.CreateDbCommand("SELECT USE_DPC FROM DB");
                customer.IsConsolidated = (bool)connector.GetRealScalarData(command);
            }
            catch(Exception)
            {
                customer.IsConsolidated = false;
                _logger.Error(() => $"Customer code {customer.CustomerCode} is not accessible");
            }

            return customer;
        }   
    }
}
