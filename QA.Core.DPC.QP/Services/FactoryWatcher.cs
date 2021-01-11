using QA.Core.DPC.QP.Models;
using QA.Core.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace QA.Core.DPC.QP.Services
{
    public class FactoryWatcher : IFactoryWatcher, IDisposable
    {
        private readonly TimeSpan _interval;
        private readonly Timer _timer;
        private readonly object _locker;
        private readonly IFactory _factory;
        private readonly ICustomerProvider _customerProvider;
        private readonly ILogger _logger;
        public event EventHandler<FactoryWatcherEventArgs> OnConfigurationModify;

        public FactoryWatcher(TimeSpan interval, IFactory factory, ICustomerProvider customerProvider, ILogger logger)
        {
            _interval = interval;
            _factory = factory;
            _customerProvider = customerProvider;
            _logger = logger;
            _timer = new Timer(OnTick, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _locker = new object();
        }     

        private void OnTick(object state)
        {
            _logger.LogInfo(() => "CustomOnTick");
            lock (_locker)
            {
                _logger.LogInfo(() => "In lock");
                var actualCustomers = new Customer[0];

                try
                {
                    _logger.LogInfo(() => "Before getting customers");
                    actualCustomers = _customerProvider.GetCustomers(out string[] notConsolidatedCodes);
                    _factory.NotConsolidatedCodes = notConsolidatedCodes;
                    _logger.LogInfo(() => "After getting customers: " + actualCustomers.Length);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Can't get customers data", ex);
                }

                var codes = _factory.CustomerMap.Keys.ToArray();
                _logger.LogInfo(() => "Codes: " + codes.Length);
                
                var actualcodes = actualCustomers.Where(c => !string.IsNullOrEmpty(c.ConnectionString))
                    .Select(c => c.CustomerCode).ToArray();
                _logger.LogInfo(() => "Actual codes: " + actualcodes.Length);                
                var deletedCodes = codes.Except(actualcodes).ToArray();
                var newcodes = actualcodes.Except(codes).ToArray();
                var modifiedCodes = _factory.CustomerMap.Join(
                        actualCustomers,
                        c => c.Key,
                        c => c.CustomerCode,
                        (c, ac) => new
                        {
                            CustomerCode = c.Key,
                            Connection = c.Value.Customer.ConnectionString,
                            ActualConnection = ac.ConnectionString
                        }
                    )
                    .Where(c => c.Connection != c.ActualConnection)
                    .Select(c => c.CustomerCode)
                    .ToArray();        

                if (deletedCodes.Any() || modifiedCodes.Any() || newcodes.Any())
                {
                    _logger.LogInfo(() => $"Customer codes changes: deleted=[{string.Join(",", deletedCodes)}]; modified=[{string.Join(",", modifiedCodes)}]; new=[{string.Join(",", newcodes)}]");
                    OnModify(deletedCodes, modifiedCodes, newcodes);
                }
            }
        }

        protected virtual void OnModify(string[] deletedCodes, string[] modifiedCodes, string[] newcodes)
        {
            var stateMap = new Dictionary<CustomerState, string[]>
            {
                { CustomerState.Deleting, deletedCodes },
                { CustomerState.Updating, modifiedCodes },
            };

            foreach(var item in stateMap)
            {
                foreach(var customerCode in item.Value)
                {
                    if (_factory.CustomerMap.TryGetValue(customerCode, out CustomerContext context))
                    {
                        context.State = item.Key;
                    }
                }
            }

            deletedCodes.Concat(modifiedCodes).ToList().ForEach(c => _factory.Clear(c));        
            newcodes.Concat(modifiedCodes).ToList().ForEach(c => _factory.Register(c));

            OnConfigurationModify?.Invoke(this, new FactoryWatcherEventArgs(deletedCodes, modifiedCodes, newcodes));
        }

        public void Start()
        {
            _timer.Change(TimeSpan.Zero, _interval);
        }

        public void Stop()
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _factory.Clear();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public void Watch()
        {
            OnTick(null);
        }
    }
}
