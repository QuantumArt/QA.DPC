using System;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using QA.Core.DPC.QP.Exceptions;
using QA.Core.DPC.QP.Models;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.QP.Services
{
    public class CustomerProvider : ICustomerProvider
    {
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
                    throw new ConsolidationException($"Customer code '{customerCode}' not found");
                }
                return new Customer(customerConfiguration);
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
                result = customers.Select(c => new Customer(c))
                    .Where(c => !onlyConsolidated || c.IsConsolidated)
                    .ToArray();
            }
            
            _logger.Info(() => $"Customers after filtering: {result.Length}");
            return result;
        }

    }
}
