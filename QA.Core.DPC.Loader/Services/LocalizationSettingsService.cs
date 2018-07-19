using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QPublishing.Database;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using Quantumart.QP8.Utils;
using System.Data.SqlClient;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC.Loader.Services
{
    public class LocalizationSettingsService : ILocalizationSettingsService
    {
        private const string QueryTemplate = @"
            SELECT
                l.Code Language,
                c.Suffix
            FROM
                CONTENT_{0}_UNITED c
                JOIN CONTENT_{1}_UNITED l ON c.Language = l.CONTENT_ITEM_ID
				LEFT JOIN CONTENT_{2}_UNITED m on c.CONTENT_ITEM_ID = m.Localization
            WHERE
				c.ARCHIVE = 0 AND
                c.VISIBLE = 1 AND
				l.ARCHIVE = 0 AND
                l.VISIBLE = 1 AND
				(m.CONTENT_ITEM_ID IS NULL OR (m.ARCHIVE = 0 AND m.VISIBLE = 1)) AND
				(m.Content IS NULL OR m.Content = @contentId)";

        private readonly ISettingsService _settingsService;
        private readonly string _connectionString;

        public LocalizationSettingsService(ISettingsService settingsService, IConnectionProvider connectionProvider)
        {
            _settingsService = settingsService;
            _connectionString = connectionProvider.GetConnection();
        }      

        public Dictionary<string, CultureInfo> GetSettings(int contentId)
        {
            return GetSettingItems(contentId)
                .ToDictionary(
                    s => string.IsNullOrEmpty(s.Suffix) ? string.Empty : s.Suffix,
                    s => string.IsNullOrEmpty(s.Language) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(s.Language)
                );
        }

        private SettingItem[] GetSettingItems(int contentId)
        {
            var cnn = QPConnectionScope.Current == null ? new DBConnector(_connectionString) : new DBConnector(QPConnectionScope.Current.DbConnection);

            var localizationContentId = _settingsService.GetSetting(SettingsTitles.LOCALIZATION_CONTENT_ID);
            var localizationMapContentId = _settingsService.GetSetting(SettingsTitles.LOCALIZATION_MAP_CONTENT_ID);
            var languagesContentId = _settingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);

            if (string.IsNullOrEmpty(localizationContentId) || string.IsNullOrEmpty(languagesContentId) || string.IsNullOrEmpty(localizationMapContentId))
            {
                return new SettingItem[0];
            }
            else
            {
                var query = string.Format(QueryTemplate, localizationContentId, languagesContentId, localizationMapContentId);
                var cmd = new SqlCommand(query);
                cmd.Parameters.AddWithValue("@contentId", contentId);

                return cnn.GetRealData(cmd)
                    .AsEnumerable()
                    .Select(row => Converter.ToModelFromDataRow<SettingItem>(row))
                    .ToArray();
            }
        }
    }

    internal class SettingItem
    {
        public string Language { get; set; }
        public string Suffix { get; set; }
    }
}
