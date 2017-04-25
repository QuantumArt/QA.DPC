using System.Linq;
using QA.Core.DPC.QP.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;

namespace QA.Core.DPC.QP.Servives
{
    public class CustomerProvider : ICustomerProvider
    {
        private readonly QPConfiguration _configuration;

        public CustomerProvider()
        {
            _configuration = new QPConfiguration();
        }

        public string GetConnectionString(string customerCode)
        {
            return QPConfiguration.GetConnectionString(customerCode);
        }

        public Customer[] GetCustomers()
        {
            return QPConfiguration.CustomerCodes
                .Select(c => new Customer
                {
                    ConnecdtionString = QPConfiguration.GetConnectionString(c),
                    CustomerCode = c
                })
                .Where(itm => IsDPCMode(itm.ConnecdtionString))
                .ToArray();
        }

        public bool IsDPCMode(string sonnectionString)
        {
            var connector = new DBConnector(sonnectionString);
            var command = new SqlCommand("SELECT USE_DPC FROM DB");
            return (bool)connector.GetRealScalarData(command);
        }
    }
}
