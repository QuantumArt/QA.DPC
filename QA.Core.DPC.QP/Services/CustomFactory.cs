using QA.Core.Logger;
using System;

namespace QA.Core.DPC.QP.Services
{
    public class CustomFactory : FactoryBase
    {
        private readonly Action<IRegistrationContext, string, string> _registration;

        public CustomFactory(Action<IRegistrationContext, string, string> registration, ICustomerProvider customerProvider, ILogger logger, bool autoRegister)
            : base(customerProvider, logger, autoRegister)
        {
            _registration = registration;
        }

        protected override void OnRegister(IRegistrationContext context, string key, string invalidationKey)
        {
            _registration?.Invoke(context, key, invalidationKey);
        }
    }
}
