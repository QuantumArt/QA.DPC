using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using Quantumart.QPublishing.Database;
using QA.Core.Cache;

namespace QA.ProductCatalog.ContentProviders.Deprecated
{
    [Obsolete(@"This class is deprecated. Please use SettingsFromQpCoreService class instead.
QA.DotNetCore.Caching.VersionedCacheCoreProvider should be registered as dependency for QA.DotNetCore.Caching.Interfaces.ICacheProvider")]
    public class SettingsFromQpCoreServiceDeprecated : SettingsServiceBaseDeprecated
    {
        private readonly VersionedCacheProviderBase _cacheProvider;

        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);

        public SettingsFromQpCoreServiceDeprecated(VersionedCacheProviderBase cacheProvider, IConnectionProvider connectionProvider)
            : base(connectionProvider, cacheProvider)
        {
            _cacheProvider = cacheProvider;
        }

        public override string GetSetting(string title)
        {
            const string key = "AllQpSettings";

            var allSettings = _cacheProvider.GetOrAdd(key, new[] { CacheTags.QP8.DB }, _cacheTimeSpan, GetAppSettings, true, CacheItemPriority.NeverRemove);

            return allSettings.ContainsKey(title) ? allSettings[title] : null;
        }

        private Dictionary<string, string> GetAppSettings()
        {
            var cnn = new DBConnector(_customer.ConnectionString, _customer.DatabaseType);
            var query = "select * from app_settings";
            return cnn.GetRealData(query).AsEnumerable()
                .ToDictionary(n => n["key"].ToString(), m => m["value"].ToString()
                );
        }
    }
}
