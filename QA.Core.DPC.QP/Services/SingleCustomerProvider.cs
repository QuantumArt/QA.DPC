using System;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public class SingleCustomerProvider : ICustomerProvider
    {
        public const string Key = "current";

        public string GetConnectionString(string customerCode)
        {
            throw new NotImplementedException();
        }

        public Customer[] GetCustomers()
        {
            return new Customer[]
            {
                new Customer
                {
                    CustomerCode = Key,
                    ConnectionString = null
                }
            };
        }
    }
}
