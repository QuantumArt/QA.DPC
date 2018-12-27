using System;
using System.Collections.Generic;
using QA.Core.DPC.QP.Services;
using ILogger = QA.Core.Logger.ILogger;
using QA.ProductCatalog.ContentProviders;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeInstanceCollection : ICustomerCodeInstanceCollection
    {
        private static readonly object Locker = new object();

        private readonly ILogger _logger;

        private readonly Dictionary<string, CustomerCodeInstance> _list = new Dictionary<string, CustomerCodeInstance>();

        private readonly Dictionary<string, CustomerCodeTaskInstance> _taskList =
            new Dictionary<string, CustomerCodeTaskInstance>();

        public CustomerCodeInstanceCollection(ILogger logger)
        {
            _logger = logger;
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
                GetCustomerCodeTaskInstance(provider, customerCode, reindexAllTaskAccessor);
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

        private CustomerCodeTaskInstance GetCustomerCodeTaskInstance(IIdentityProvider provider, string customerCode, Func<ITask> reindexAllTaskAccessor)
        {
            CustomerCodeTaskInstance result = null;
            if (customerCode == null || _taskList.TryGetValue(customerCode, out result)) return result;
            lock (Locker)
            {
                if (!_taskList.TryGetValue(customerCode, out result))
                {
                    result = new CustomerCodeTaskInstance(provider, reindexAllTaskAccessor(), _logger);
                    _taskList.Add(customerCode, result);
                }
            }
            return result;
        }
    }
}
