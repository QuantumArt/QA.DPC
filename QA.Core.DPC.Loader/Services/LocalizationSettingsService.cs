using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using Quantumart.QPublishing.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Quantumart.QP8.Utils;

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
            WHERE
				c.ARCHIVE = 0 AND
                c.VISIBLE = 1";

        private readonly ISettingsService _settingsService;

        public LocalizationSettingsService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }      

        public Dictionary<string, CultureInfo> GetSettings(int definitionId)
        {
            var x = GetSettingItems();

            return GetSettingItems()
                .ToDictionary(
                    s => s.Suffix,
                    s => CultureInfo.GetCultureInfo(s.Language)
                );
        }

        private SettingItem[] GetSettingItems()
        {
            var cnn = QPConnectionScope.Current == null ? new DBConnector() : new DBConnector(QPConnectionScope.Current.DbConnection);

            var localizationContentId = _settingsService.GetSetting(SettingsTitles.LOCALIZATION_CONTENT_ID);
            var languagesContentId = _settingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);

            if (string.IsNullOrEmpty(localizationContentId) || string.IsNullOrEmpty(languagesContentId))
            {
                return new SettingItem[0];
            }
            else
            {
                var query = string.Format(QueryTemplate, localizationContentId, languagesContentId);

                return cnn.GetRealData(query)
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
