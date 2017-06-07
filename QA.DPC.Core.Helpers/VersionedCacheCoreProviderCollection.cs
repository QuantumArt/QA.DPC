using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.QP.Services;
using QA.Core;
using ILogger = QA.Core.ILogger;

namespace QA.DPC.Core.Helpers
{

    public interface IVersionedCacheCoreProviderCollection
    {
        VersionedCacheCoreProvider Get(IIdentityProvider provider);
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

        public VersionedCacheCoreProvider Get(IIdentityProvider provider)
        {
            
            VersionedCacheCoreProvider result = null;
            var customerCode = provider.Identity?.CustomerCode;
            if (customerCode != null && !_list.TryGetValue(customerCode, out result))
            {
                lock (Locker)
                {
                    result = new VersionedCacheCoreProvider(_logger);
                    _list.Add(customerCode, result);
                }

            }
            return result;
        }
    }
}
