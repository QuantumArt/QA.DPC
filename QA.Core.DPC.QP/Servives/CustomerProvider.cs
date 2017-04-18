using System.Linq;
using QA.Core.DPC.QP.Models;
using Quantumart.QP8.Configuration;

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
                .ToArray();
        }
    }
}
