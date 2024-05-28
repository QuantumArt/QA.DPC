using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QA.Core.DPC.QP.Cache;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders.Deprecated
{
    public class SettingsFromQpCoreService : SettingsServiceBase
    {
        private readonly TimeSpan _cacheTimeSpan = TimeSpan.FromMinutes(5);

        public SettingsFromQpCoreService(
            IConnectionProvider connectionProvider,
            ICacheProvider cacheProvider)
            : base(connectionProvider, cacheProvider)
        {

        }

        public override string GetSetting(string title)
        {
            const string key = "AllQpSettings";
            var allSettings = CacheProvider.GetOrAdd(key, new[] { CacheTags.QP8.DB }, _cacheTimeSpan, GetAppSettings);
            return allSettings.ContainsKey(title) ? allSettings[title] : null;
        }

        private Dictionary<string, string> GetAppSettings()
        {
            var cnn = _customer.DbConnector;
            var query = "select * from app_settings";
            return cnn.GetRealData(query).AsEnumerable()
                .ToDictionary(n => n["key"].ToString(), m => m["value"].ToString());
        }
    }
}
