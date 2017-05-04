using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Servives
{
    public class SingleCustomerProvider : ICustomerProvider
    {
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
                    CustomerCode = "current",
                    ConnectionString = null
                }
            };
        }
    }
}
