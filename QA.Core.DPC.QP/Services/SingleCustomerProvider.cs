using QA.Core.DPC.QP.Models;
using System;
using System.Configuration;
using QP.ConfigurationService.Models;

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

            throw new InvalidOperationException();
        }

        public Customer GetCustomer(string customerCode)
        {
            if (customerCode == Key)
            {
                return new Customer
                {
                    ConnectionString = ConfigurationManager.ConnectionStrings["qp_database"]?.ConnectionString,
                    DatabaseType = ConfigurationManager.AppSettings["usePostgres"] == "true" ? DatabaseType.Postgres : DatabaseType.SqlServer
                };
            }

            throw new InvalidOperationException();
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
