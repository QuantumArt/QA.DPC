using QA.Core.DPC.QP.Services;
using QA.Core.DPC.QP.Servives;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Notification.Services
{
	public class NotificationChannelProvider : ContentProviderBase<NotificationChannel>
	{
		#region Constants
		private const string QueryTemplate = @"
			SELECT
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
				CONTENT_{0}_UNITED c
				join CONTENT_{1}_UNITED f ON c.[Format] = f.CONTENT_ITEM_ID
			WHERE
				c.ARCHIVE = 0 AND c.VISIBLE = 1 AND f.ARCHIVE = 0 AND f.VISIBLE = 1";

        private const string QueryLangTemplate = @"
			SELECT
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
	            CONTENT_{0}_UNITED c
	            join CONTENT_{1}_UNITED f ON c.[Format] = f.CONTENT_ITEM_ID
	            join CONTENT_{2}_UNITED l on c.[Language] = l.CONTENT_ITEM_ID
            WHERE
	            c.ARCHIVE = 0 AND
	            c.VISIBLE = 1 AND
	            f.ARCHIVE = 0 AND
	            f.VISIBLE = 1 AND
	            l.ARCHIVE = 0 AND
	            l.VISIBLE = 1";
        #endregion

        public NotificationChannelProvider(ISettingsService settingsService, IConnectionProvider connectionProvider)
			: base(settingsService, connectionProvider)
		{
		}

		#region Overrides
		protected override string GetQuery()
		{
			var channelsContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID);
			var formattersContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_FORMATTERS_CONTENT_ID);
            var languagesContentId = SettingsService.GetSetting(SettingsTitles.LANGUAGES_CONTENT_ID);

            if (string.IsNullOrEmpty(channelsContentId) || string.IsNullOrEmpty(formattersContentId))
			{
				return null;
			}
			else
			{
                if (string.IsNullOrEmpty(languagesContentId))
                {
                    return string.Format(QueryTemplate, channelsContentId, formattersContentId);
                }
                else
                {
                    return string.Format(QueryLangTemplate, channelsContentId, formattersContentId, languagesContentId);
                }
			}
		}
		#endregion
	}
}
