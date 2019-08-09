using QA.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC.QP.Services
{
    public abstract class FactoryBase : IFactory, IRegistrationContext, IDisposable
    {
        private readonly ILogger _logger;
        private readonly ICustomerProvider _customerProvider;
        private readonly Dictionary<string, Dictionary<Type, object>> _container;
        private readonly bool _autoRegister;
        private readonly object _locker;
        public Dictionary<string, string> Invalidator { get; private set; }

        public FactoryBase(ICustomerProvider customerProvider, ILogger logger, bool autoRegister)
        {
            _logger = logger;
            _customerProvider = customerProvider;
            _autoRegister = autoRegister;
            _container = new Dictionary<string, Dictionary<Type, object>>();
            Invalidator = new Dictionary<string, string>();
            _locker = new object();
        }

        void IRegistrationContext.Register<T>(string key, T value)
        {            
            if (!_container.TryGetValue(key, out var internalContainer))
            {
                internalContainer = new Dictionary<Type, object>();
                _container[key] = internalContainer;
            }

            internalContainer[typeof(T)] = value;
            _logger.LogInfo(() => $"Register {typeof(T).Name} as {value.GetType().Name} for {key}");
        }

        public void Register(string key)
        {
            lock (_locker)
            {
                _logger.LogInfo(() => $"Start register factory for {key}");

                if (!string.IsNullOrEmpty(key))
                {
                    var customer = _customerProvider.GetCustomer(key);
                    Invalidator[key] = customer.ConnectionString;
                    OnRegister((IRegistrationContext)this , customer);
                }
                _logger.LogInfo(() => $"End register for {key}");
            }
        }

        protected abstract void OnRegister(IRegistrationContext context, Customer customer);

        protected T Resolve<T>(string key, bool autoRegister)
        {
            if (_container.TryGetValue(key, out Dictionary<Type, object> internalContainer))
            {
                if (internalContainer.TryGetValue(typeof(T), out object value))
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

            throw new Exception($"Can't resolve {typeof(T).Name} for {key}");
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

                if (Invalidator.ContainsKey(key))
                {
                    Invalidator.Remove(key);
                }

                if (_container.TryGetValue(key, out Dictionary<Type, object> internalContainer))
                {
                    internalContainer.Values
                        .Distinct()
                        .OfType<IDisposable>()
                        .ToList()
                        .ForEach(x => x.Dispose());

                    _container.Remove(key);
                }
            }
        }

        public void Clear()
        {
            _container.Keys
                .ToList()
                .ForEach(Clear);

            Invalidator
                .Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
