using System;
using QA.Core;
using QA.Core.Cache;
using QA.Core.Data;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeInstance
    {
        public IVersionedCacheProvider2 CacheProvider { get; set; }

        public IContentInvalidator Invalidator { get; set; }

        public CacheItemTracker Tracker { get; set; }

        public ICacheItemWatcher Watcher { get; set; }

        public CustomerCodeInstance(IConnectionProvider connectionProvider, ILogger logger)
        {
            CacheProvider = new VersionedCacheCoreProvider(logger);
            Invalidator = new DpcContentInvalidator(CacheProvider, logger);

            if (!String.IsNullOrEmpty(connectionProvider.GetConnection()))
            {
                Watcher = new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15),
                    Invalidator, connectionProvider, logger);
                Tracker = new StructureCacheTracker(connectionProvider);
                Watcher.AttachTracker(Tracker);
                ((CustomerCacheItemWatcher)Watcher).Start();
            }
        }
    }
}