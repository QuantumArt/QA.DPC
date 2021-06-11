using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Collections.Generic;
using ILogger = QA.Core.Logger.ILogger;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeInstanceCollection : ICustomerCodeInstanceCollection
    {
        private static readonly object Locker = new object();

        private readonly ILogger _logger;

        private readonly TaskRunnerDelays _delays;

        private readonly Dictionary<string, CustomerCodeInstance> _list = new Dictionary<string, CustomerCodeInstance>();

        private readonly Dictionary<string, CustomerCodeTaskInstance> _taskList =
            new Dictionary<string, CustomerCodeTaskInstance>();

        private readonly IFactory _consolidationFactory;

        public CustomerCodeInstanceCollection(ILogger logger, TaskRunnerDelays delays, IFactory consolidationFactory)
        {
            _logger = logger;
            _delays = delays;
            _consolidationFactory = consolidationFactory;
        }

        public CustomerCodeInstance Get(IIdentityProvider provider, IConnectionProvider connectionProvider)
        {
            var customerCode = provider.Identity?.CustomerCode;
            CustomerCodeInstance result =
                GetCustomerCodeInstance(connectionProvider, customerCode);
            return result;
        }

        public CustomerCodeTaskInstance Get(IIdentityProvider provider, Func<ITask> reindexAllTaskAccessor)
        {
            var customerCode = provider.Identity?.CustomerCode;
            CustomerCodeTaskInstance result =
                GetCustomerCodeTaskInstance(provider, customerCode, reindexAllTaskAccessor, _delays);
            return result;
        }

        private CustomerCodeInstance GetCustomerCodeInstance(IConnectionProvider connectionProvider, string customerCode)
        {
            CustomerCodeInstance result = null;
            if (customerCode == null || _list.TryGetValue(customerCode, out result)) return result;
            lock (Locker)
            {
                if (!_list.TryGetValue(customerCode, out result))
                {
                    result = new CustomerCodeInstance(connectionProvider, _logger);
                    _list.Add(customerCode, result);
                }
            }
            return result;
        }

        private CustomerCodeTaskInstance GetCustomerCodeTaskInstance(
            IIdentityProvider provider, 
            string customerCode, 
            Func<ITask> reindexAllTaskAccessor, 
            TaskRunnerDelays delays
        )
        {
            CustomerCodeTaskInstance result = null;
            if (customerCode == null || _taskList.TryGetValue(customerCode, out result)) return result;
            lock (Locker)
            {
                if (!_taskList.TryGetValue(customerCode, out result))
                {
                    result = new CustomerCodeTaskInstance(provider, reindexAllTaskAccessor(), delays, _consolidationFactory);
                    _taskList.Add(customerCode, result);
                }
            }
            return result;
        }
    }
}
