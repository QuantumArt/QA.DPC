
using QA.Core.DPC.QP.Models;
using System;
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
            if (customerCode != Key)
            {
                throw new InvalidOperationException();
            }
            return _cnnProps.DpcConnectionString;
        }

        public Customer GetCustomer(string customerCode)
        {
            if (customerCode != Key)
            {
                throw new InvalidOperationException();
            }
            return new Customer(_cnnProps.DpcConnectionString, Key, _cnnProps.GetDatabaseType());
        }

        public Customer[] GetCustomers(bool onlyConsolidated = true)
        {
            return new[]
            {
                new Customer(_cnnProps.DpcConnectionString, Key, _cnnProps.GetDatabaseType())
                {
                    IsConsolidated = true
                }
            };
        }
    }
}