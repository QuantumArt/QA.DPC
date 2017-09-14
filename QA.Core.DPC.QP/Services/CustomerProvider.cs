using System;
using System.Data.SqlClient;
using System.Linq;
using QA.Core.DPC.QP.Models;
using QA.Core.Logger;
using Quantumart.QP8.Configuration;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.QP.Services
{
    public class CustomerProvider : ICustomerProvider
    {
        private const int Timeout = 2;
        public QPConfiguration Configuration1 { get; }
        private readonly ILogger _logger;

        public CustomerProvider(ILogger logger)
        {
            Configuration1 = new QPConfiguration();
            _logger = logger;
        }

        public string GetConnectionString(string customerCode)
        {
            return QPConfiguration.GetConnectionString(customerCode);
        }

        public Customer[] GetCustomers()
        {
            return QPConfiguration.GetCustomerCodes()
                .Select(c => new Customer
                {
                    ConnectionString = QPConfiguration.GetConnectionString(c),
                    CustomerCode = c
                })
                .Where(IsDpcMode)
                .ToArray();
        }

        public bool IsDpcMode(Customer customer)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(customer.ConnectionString) {ConnectTimeout = Timeout};
                var connector = new DBConnector(builder.ConnectionString);
                var command = new SqlCommand("SELECT USE_DPC FROM DB");
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
