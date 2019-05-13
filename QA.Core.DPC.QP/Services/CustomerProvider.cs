using System;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using QA.Core.DPC.QP.Models;
using QA.Core.Logger;
using Quantumart.QPublishing.Database;

namespace QA.Core.DPC.QP.Services
{
    public class CustomerProvider : ICustomerProvider
    {
        private const int Timeout = 2;
        private readonly ILogger _logger;

        public CustomerProvider(ILogger logger)
        {
            _logger = logger;
        }

        public string GetConnectionString(string customerCode)
        {
            return DBConnector.GetConnectionString(customerCode);
        }

        public Customer[] GetCustomers()
        {
            var result = new Customer[] { };
            var doc = XDocument.Parse(DBConnector.GetQpConfig().OuterXml);
            var customer = doc?.Root?.Element("customers");
            if (customer != null)
            {
                result =
                    customer.Elements("customer").Select(c => new Customer
                    {
                        ConnectionString = c.Element("db")?.Value?.Replace("Provider=SQLOLEDB;", string.Empty),
                        CustomerCode = c.Attribute("customer_name")?.Value
                    })
                    .Where(IsDpcMode)
                    .ToArray();             
            }

            return result;
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
