using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Collections.Generic;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeTaskInstanceCollection : ICustomerCodeTaskInstanceCollection
    {
        private static readonly object Locker = new object();

        private readonly TaskRunnerDelays _delays;

        private readonly Dictionary<string, CustomerCodeTaskInstance> _taskList =
            new Dictionary<string, CustomerCodeTaskInstance>();

        private readonly IFactory _consolidationFactory;

        public CustomerCodeTaskInstanceCollection(TaskRunnerDelays delays, IFactory consolidationFactory)
        {
            _delays = delays;
            _consolidationFactory = consolidationFactory;
        }

        public CustomerCodeTaskInstance Get(IIdentityProvider provider, ITask reindexAllTask)
        {
            var customerCode = provider.Identity?.CustomerCode;
            CustomerCodeTaskInstance result =
                GetCustomerCodeTaskInstance(provider, customerCode, reindexAllTask, _delays);
            return result;
        }

        private CustomerCodeTaskInstance GetCustomerCodeTaskInstance(
            IIdentityProvider provider, 
            string customerCode, 
            ITask reindexAllTask, 
            TaskRunnerDelays delays
        )
        {
            CustomerCodeTaskInstance result = null;
            if (customerCode == null || _taskList.TryGetValue(customerCode, out result)) return result;
            lock (Locker)
            {
                if (!_taskList.TryGetValue(customerCode, out result))
                {
                    result = new CustomerCodeTaskInstance(provider, reindexAllTask, delays, _consolidationFactory);
                    _taskList.Add(customerCode, result);
                }
            }
            return result;
        }
    }
}
