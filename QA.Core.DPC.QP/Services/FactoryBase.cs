using QA.Core.DPC.QP.Exceptions;
using QA.Core.DPC.QP.Models;
using QA.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QA.Core.DPC.QP.Services
{
    public abstract class FactoryBase : IFactory, IRegistrationContext, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICustomerProvider _customerProvider;
        private readonly bool _autoRegister;
        private readonly object _locker;
        public string[] NotConsolidatedCodes { get; set; }
        public Dictionary<string, CustomerContext> CustomerMap { get; private set; }

        public FactoryBase(ICustomerProvider customerProvider, ILogger logger, bool autoRegister)
        {
            _logger = logger;
            _customerProvider = customerProvider;
            _autoRegister = autoRegister;
            CustomerMap = new Dictionary<string, CustomerContext>();
            _locker = new object();
        }

        void IRegistrationContext.Register<T>(string key, T value)
        {            
            if (CustomerMap.TryGetValue(key, out var context))
            {
                context.Container[typeof(T)] = value;
                _logger.LogInfo(() => $"Register {typeof(T).Name} as {value.GetType().Name} for {key}");
            }
            else
            {
                throw new ConsolidationException($"Can't register {typeof(T).Name} for {key}");
            }
        }

        public void Register(string key)
        {
            lock (_locker)
            {
                _logger.LogInfo(() => $"Start register factory for {key}");

                if (!string.IsNullOrEmpty(key))
                {
                    var customer = _customerProvider.GetCustomer(key);
                    var context = new CustomerContext(customer);
                    context.State = CustomerState.Creating;
                    CustomerMap[key] = context;
                    OnRegister((IRegistrationContext)this , context.Customer);
                    context.State = CustomerState.Active;
                    _logger.LogInfo(() => $"End register for {key}");
                }
                else
                {
                    _logger.LogInfo(() => $"{key} is not registered");
                }
            }
        }

        protected abstract void OnRegister(IRegistrationContext context, Customer customer);

        protected T Resolve<T>(string key, bool autoRegister)
        {
            if (CustomerMap.TryGetValue(key, out CustomerContext context))
            {
                if (context.Container.TryGetValue(typeof(T), out object value))
                {
                    if (value is T)
                    {
                        return (T)value;
                    }
                }
            }
            else if (autoRegister)
            {
                Register(key);
                return Resolve<T>(key, false);
            }

            throw new ConsolidationException($"Can't resolve {typeof(T).Name} for {key}");
        }

        public T Resolve<T>(string key)
        {
            lock (_locker)
            {
                return Resolve<T>(key, _autoRegister);
            }
        }
   
        public void Clear(string key)
        {
            lock (_locker)
            {
                _logger.LogInfo(() => $"Clear factory for {key}");


                if (CustomerMap.TryGetValue(key, out CustomerContext context))
                {
                    context.Container.Values
                        .Distinct()
                        .OfType<IDisposable>()
                        .ToList()
                        .ForEach(x => x.Dispose());

                    CustomerMap.Remove(key);
                }
            }
        }

        public void Clear()
        {
            CustomerMap.Keys
                .ToList()
                .ForEach(Clear);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
