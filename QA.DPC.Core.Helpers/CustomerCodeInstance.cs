using System;
using QA.Core;
using QA.Core.Cache;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeInstance
    {
        public VersionedCacheProviderBase CacheProvider { get; set; }

        public IContentInvalidator Invalidator { get; set; }

        public CacheItemTracker Tracker { get; set; }

        public ICacheItemWatcher Watcher { get; set; }

        public CustomerCodeInstance(IConnectionProvider connectionProvider, ILogger logger)
        {
            CacheProvider = new VersionedCacheProviderBase(logger);
            Invalidator = new DpcContentInvalidator(CacheProvider, logger);

            var customer = connectionProvider.GetCustomer();
            if (customer != null)
            {
                Watcher = new CustomerCoreCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15),
                    Invalidator, connectionProvider, logger, databaseType: customer.DatabaseType);
                Tracker = new StructureCacheTracker(customer.ConnectionString, customer.DatabaseType);
                Watcher.AttachTracker(Tracker);
                ((CacheItemWatcherBase)Watcher).Start();
            }
        }
    }
}