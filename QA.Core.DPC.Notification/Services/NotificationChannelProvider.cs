using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Notification.Services
{
	public class NotificationChannelProvider : ContentProviderBaseCached<NotificationChannel>
	{
        #region Constants
        private const string AutopublishField = "c.Autopublish,";

        private const string NoAutopublishField = "0 Autopublish,";

        private const string QueryTemplate = @"
			SELECT {0}
				c.Name,
				c.IsStage,
				c.Url,
				c.DegreeOfParallelism,
				c.Filter,
				f.Name [Format],
				f.Formatter,
				f.MediaType,
                '' Language
			FROM
				CONTENT_{1}_UNITED c
				join CONTENT_{2}_UNITED f ON c.[Format] = f.CONTENT_ITEM_ID
			WHERE
				c.ARCHIVE = 0 AND c.VISIBLE = 1 AND f.ARCHIVE = 0 AND f.VISIBLE = 1";

        private const string QueryLangTemplate = @"
			SELECT {0}
	            c.Name,
	            c.IsStage,
	            c.Url,
	            c.DegreeOfParallelism,
	            c.Filter,
	            f.Name [Format],
	            f.Formatter,
	            f.MediaType,
	            CASE WHEN l.Code IS NULL
		            THEN ''
		            ELSE l.Code
	            END [Language]
            FROM
	            CONTENT_{1}_UNITED c
	            join CONTENT_{2}_UNITED f ON c.[Format] = f.CONTENT_ITEM_ID
	            join CONTENT_{3}_UNITED l on c.[Language] = l.CONTENT_ITEM_ID
            WHERE
	            c.ARCHIVE = 0 AND
	            c.VISIBLE = 1 AND
	            f.ARCHIVE = 0 AND
	            f.VISIBLE = 1 AND
	            l.ARCHIVE = 0 AND
	            l.VISIBLE = 1";
        #endregion

        public NotificationChannelProvider(ISettingsService settingsService, IConnectionProvider connectionProvider, IVersionedCacheProvider cacheProvider)
			: base(settingsService, connectionProvider, cacheProvider)
		{
		}

		#region Overrides
		protected override string GetQuery()
		{
			var channelsContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID);
			var formattersContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_FORMATTERS_CONTENT_ID);
            var languagesContentId = SettingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);
            var autopublish = GetBoolValue(SettingsTitles.NOTIFICATION_SENDER_AUTOPUBLISH, false);
            var autopublishField = autopublish ? AutopublishField : NoAutopublishField;

            if (string.IsNullOrEmpty(channelsContentId) || string.IsNullOrEmpty(formattersContentId))
			{
				return null;
			}
			else
			{
                if (string.IsNullOrEmpty(languagesContentId))
                {
                    return string.Format(QueryTemplate, autopublishField, channelsContentId, formattersContentId);
                }
                else
                {
                    return string.Format(QueryLangTemplate, autopublishField, channelsContentId, formattersContentId, languagesContentId);
                }
			}
		}

	    public override string[] GetTags()
	    {
	        return new[] { SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID) };
	    }

        #endregion

        #region Private
        private bool GetBoolValue(SettingsTitles title, bool defaultValue)
        {
            var value = SettingsService.GetSetting(title);

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }
            else
            {
                return bool.Parse(value);
            }
        }
        #endregion

		protected override string CacheKey => "NotificationChannel";
	}
}
