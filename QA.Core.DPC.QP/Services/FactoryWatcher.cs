using System;
using System.Linq;
using System.Threading;
using QA.Core.DPC.QP.Models;
using QA.Core.Logger;

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
            lock (_locker)
            {
                var actualCustomers = new Customer[0];

                try
                {
                    actualCustomers = _customerProvider.GetCustomers();
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("Can't get customers data", ex);
                }

                var codes = _factory.Invalidator.Keys.ToArray();
                var actualcodes = actualCustomers.Select(c => c.CustomerCode).ToArray();
                var deletedCodes = codes.Except(actualcodes).ToArray();
                var newcodes = actualcodes.Except(codes).ToArray();
                var modifiedCodes = _factory.Invalidator.Join(
                        actualCustomers,
                        c => c.Key,
                        c => c.CustomerCode,
                        (c, ac) => new
                        {
                            CustomerCode = c.Key,
                            Connection = c.Value,
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
