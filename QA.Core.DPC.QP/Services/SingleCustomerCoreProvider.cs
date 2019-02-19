
using QA.Core.DPC.QP.Models;
using System;
using System.Configuration;
using Microsoft.Extensions.Options;

namespace QA.Core.DPC.QP.Services
{
    public class SingleCustomerCoreProvider : ICustomerProvider
    {
        public const string Key = "current";

        private ConnectionProperties _cnnProps;

        public SingleCustomerCoreProvider(IOptions<ConnectionProperties> cnnProps)
        {
            _cnnProps = cnnProps.Value;
        }

        public string GetConnectionString(string customerCode)
        {
            if (customerCode == Key)
            {
                return _cnnProps.DpcConnectionString;
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