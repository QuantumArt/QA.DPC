using QA.Core.Logger;
using System;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public class CustomFactory : FactoryBase
    {
        private readonly Action<IRegistrationContext, Customer> _registration;

        public CustomFactory(Action<IRegistrationContext, Customer> registration, ICustomerProvider customerProvider, ILogger logger, bool autoRegister)
            : base(customerProvider, logger, autoRegister)
        {
            _registration = registration;
        }

        protected override void OnRegister(IRegistrationContext context, Customer customer)
        {
            _registration?.Invoke(context, customer);
        }
    }
}
