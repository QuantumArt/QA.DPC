using System;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders.Deprecated
{
    public class SettingsFromContentCoreService : SettingsServiceBase
    {
        private const string FIELD_NAME_TITLE = "Title";
        private const string FIELD_NAME_VALUE = "Value";

        private readonly int _settingsContentId;
        private readonly TimeSpan _cachePeriod = new TimeSpan(3, 10, 0);

        public SettingsFromContentCoreService(
            IConnectionProvider connectionProvider,
            ICacheProvider cacheProvider,
            int settingsContentId)
            : base(connectionProvider, cacheProvider)
        {
            _settingsContentId = settingsContentId;
        }

        public override string GetSetting(string title)
        {
            var key = string.Format("GetSetting_{0}", title);
            return CacheProvider.GetOrAdd(
                key,
                new[] { _settingsContentId.ToString() },
                _cachePeriod,
                () => GetSettingValue(title));
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
