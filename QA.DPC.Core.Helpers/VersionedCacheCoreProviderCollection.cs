using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.QP.Services;
using QA.Core;
using QA.Core.Data;
using QA.Core.DPC.Loader;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Cache;
using ILogger = QA.Core.ILogger;

namespace QA.DPC.Core.Helpers
{

    public interface IVersionedCacheCoreProviderCollection
    {
        VersionedCacheCoreProvider Get(IIdentityProvider provider, IConnectionProvider connectionProvider);
    }

    public class VersionedCacheCoreProviderCollection : IVersionedCacheCoreProviderCollection
    {
        private static readonly object Locker = new object();

        private readonly ILogger _logger;

        private readonly Dictionary<string, VersionedCacheCoreProvider> _list = new Dictionary<string, VersionedCacheCoreProvider>();

        public VersionedCacheCoreProviderCollection(ILogger logger)
        {
            _logger = logger;
        }

        public VersionedCacheCoreProvider Get(IIdentityProvider provider, IConnectionProvider connectionProvider)
        {
            VersionedCacheCoreProvider result = null;
            var customerCode = provider.Identity?.CustomerCode;
            if (customerCode != null && !_list.TryGetValue(customerCode, out result))
            {
                lock (Locker)
                {
                    result = new VersionedCacheCoreProvider(_logger);
                    var invalidator = new DpcContentInvalidator(result, _logger);
                    var a = new CustomerCacheItemWatcher(InvalidationMode.All, TimeSpan.FromSeconds(15), invalidator, connectionProvider, _logger);
                    var tracker = new StructureCacheTracker(connectionProvider);
                    a.AttachTracker(tracker);
                    a.Start();
                    _list.Add(customerCode, result);
                }

            }
            return result;
        }
    }
}
