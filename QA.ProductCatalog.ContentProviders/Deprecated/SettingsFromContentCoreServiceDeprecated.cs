﻿using System;
using Microsoft.Extensions.Caching.Memory;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders.Deprecated
{
    [Obsolete(@"This class is deprecated. Please use SettingsFromContentCoreService class instead.
QA.DotNetCore.Caching.VersionedCacheCoreProvider should be registered as dependency for QA.DotNetCore.Caching.Interfaces.ICacheProvider")]
    public class SettingsFromContentCoreServiceDeprecated : SettingsServiceBaseDeprecated
    {
        private readonly VersionedCacheProviderBase _cacheProvider;

        private readonly int _settingsContentId;

        public SettingsFromContentCoreServiceDeprecated(VersionedCacheProviderBase cacheProvider,
            IConnectionProvider connectionProvider,
            int settingsContentId)
            : base(connectionProvider, cacheProvider)
        {
            _cacheProvider = cacheProvider;
            _settingsContentId = settingsContentId;
        }

        private const string FIELD_NAME_TITLE = "Title";

        private const string FIELD_NAME_VALUE = "Value";

        private readonly TimeSpan _cachePeriod = new TimeSpan(3, 10, 0);

        public override string GetSetting(string title)
        {
            var key = string.Format("GetSetting_{0}", title);
            return _cacheProvider.GetOrAdd(key, new[] { _settingsContentId.ToString() }, _cachePeriod,
                () => GetSettingValue(title), true, CacheItemPriority.NeverRemove);
        }

        private string GetSettingValue(string title)
        {
            var cnn = _customer.DbConnector;
            var keycolumn = FIELD_NAME_TITLE;
            var valuecolumn = FIELD_NAME_VALUE;
            var keyvalue = title.Replace("'", "''");
            var query = $"select {valuecolumn} from content_{_settingsContentId}_united" +
                        $" where archive = 0 and visible = 1 and {keycolumn} = '{keyvalue}'";
            var data = cnn.GetData(query);
            return data.Rows.Count > 0 ? data.Rows[0][valuecolumn].ToString() : null;
        }
    }
}
