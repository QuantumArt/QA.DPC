using System;
using QA.Core.DPC.QP.Services;
using QA.DotNetCore.Caching.Interfaces;
using Quantumart.QPublishing.Database;

namespace QA.ProductCatalog.ContentProviders
{
    public class SettingsFromContentCoreService : SettingsServiceBase
    {
        private const string FieldNameTitle = "Title";
        private const string FieldNameValue = "Value";

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
            var key = $"GetSetting_{title}";
            return CacheProvider.GetOrAdd(
                key,
                new[] { _settingsContentId.ToString() },
                _cachePeriod,
                () => GetSettingValue(title));
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
