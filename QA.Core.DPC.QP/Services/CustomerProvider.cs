using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.Options;
using Npgsql;
using QA.Core.DPC.QP.Exceptions;
using QA.Core.DPC.QP.Models;
using QA.Core.Logger;
using QP.ConfigurationService.Models;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.QP.Services
{
    public class CustomerProvider : ICustomerProvider
    {
        private const int Timeout = 2;
        private readonly ILogger _logger;
        private readonly IntegrationProperties _integrationProps;

        public CustomerProvider(ILogger logger, IOptions<IntegrationProperties> integrationProps)
        {
            _logger = logger;
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

        public Customer[] GetCustomers(out string[] notConsolidatedCodes)
        {
            var result = new Customer[] { };
            notConsolidatedCodes = new string[0];
            DBConnector.ConfigServiceUrl = _integrationProps.ConfigurationServiceUrl;
            DBConnector.ConfigServiceToken = _integrationProps.ConfigurationServiceToken;
            _logger.LogInfo(() => $"Config service url :{DBConnector.ConfigServiceUrl}");
            var configuration = DBConnector.GetQpConfiguration().Result;
            var customers = configuration.Customers;
            _logger.LogInfo(() => $"Received customers: {customers.Length}");
            if (customers != null)
            {
                result =
                    customers.Select(c => new Customer
                        {
                            ConnectionString = c.ConnectionString.Replace("Provider=SQLOLEDB;", ""),
                            CustomerCode = c.Name,
                            DatabaseType = c.DbType
                        })
                        .Where(IsDpcMode)
                        .ToArray();

                var consolidatedCodes = result.Select(c => c.CustomerCode).ToArray();
                var allCodes = customers.Select(c => c.Name).ToArray();
                notConsolidatedCodes = allCodes.Except(consolidatedCodes).ToArray();
            }
            
            _logger.LogInfo(() => $"Customers after filtering: {result.Length}");
            return result;
        }

        public Customer[] GetCustomers()
        {
            return GetCustomers(out string[] notConsolidatedCodes);
        }

        public bool IsDpcMode(Customer customer)
        {
            try
            {
                var builder = customer.DatabaseType == DatabaseType.SqlServer
                    ? (DbConnectionStringBuilder) new SqlConnectionStringBuilder(customer.ConnectionString)
                        {ConnectTimeout = Timeout}
                    : new NpgsqlConnectionStringBuilder(customer.ConnectionString) {CommandTimeout = Timeout};
                var connector = new DBConnector(builder.ConnectionString, customer.DatabaseType);
                var command =  connector.CreateDbCommand("SELECT USE_DPC FROM DB");
                return (bool)connector.GetRealScalarData(command);
            }
            catch(Exception)
            {
                _logger.LogError(() => $"Customer code {customer.CustomerCode} is not accessible");
                return false;
            }
        }   
    }
}
