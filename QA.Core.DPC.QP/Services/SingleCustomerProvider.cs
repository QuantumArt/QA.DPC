using QA.Core.DPC.QP.Models;
using System;
using System.Configuration;

namespace QA.Core.DPC.QP.Services
{
    public class SingleCustomerProvider : ICustomerProvider
    {
        public const string Key = "current";

        public string GetConnectionString(string customerCode)
        {
            if (customerCode == Key)
            {
                return ConfigurationManager.ConnectionStrings["qp_database"]?.ConnectionString;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public Customer[] GetCustomers()
        {
            return new Customer[]
            {
                new Customer
                {
                    CustomerCode = Key,
                    ConnectionString = GetConnectionString(Key)
                }
            };
        }
    }
}
