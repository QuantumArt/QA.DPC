using System;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Servives
{
    public class CustomerProvider : ICustomerProvider
    {
        public CustomerProvider()
        {

        }

        public string GetConnectionString(string customerCode)
        {
            return "";
        }

        public Customer[] GetCustomers()
        {
            throw new NotImplementedException();
        }
    }
}
