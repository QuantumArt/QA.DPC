using System;
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
        private const string FieldNameTitle = "Title";
        private const string FieldNameValue = "Value";
        private readonly TimeSpan _cachePeriod = new TimeSpan(3, 10, 0);
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

        public override string GetSetting(string title)
        {
            var key = $"GetSetting_{title}";
            return _cacheProvider.GetOrAdd(key, new[] { _settingsContentId.ToString() }, _cachePeriod,
                () => GetSettingValue(title), true, CacheItemPriority.NeverRemove);
        }

        private string GetSettingValue(string title)
        {
            var cnn = _customer.DbConnector;
            var query = $"select {FieldNameValue} from content_{_settingsContentId}_united" +
                        $" where archive = 0 and visible = 1 and {FieldNameTitle} = @title";
            var dbCommand = cnn.CreateDbCommand(query);
            dbCommand.Parameters.AddWithValue("@title", title);
            var data = cnn.GetRealData(dbCommand);
            return data.Rows.Count > 0 ? data.Rows[0][FieldNameValue].ToString() : null;
        }
    }
}
