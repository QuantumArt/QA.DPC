using System;
using System.Collections.Generic;

namespace QA.Core.DPC.QP.Models
{
    public class CustomerContext
    {
        public Customer Customer { get; set; }
        public CustomerState State { get; set; }
        public Dictionary<Type, object> Container { get; set; }


        public CustomerContext(Customer customer)
        {
            Customer = customer;
            State = CustomerState.Creating;
            Container = new Dictionary<Type, object>();
        }

    }
}
