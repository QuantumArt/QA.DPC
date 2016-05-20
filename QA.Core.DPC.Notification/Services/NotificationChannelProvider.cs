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
				f.MediaType
			FROM
				CONTENT_{0}_UNITED c
				join CONTENT_{1}_UNITED f ON c.[Format] = f.CONTENT_ITEM_ID
			WHERE
				c.ARCHIVE = 0 AND c.VISIBLE = 1 AND f.ARCHIVE = 0 AND f.VISIBLE = 1";
		#endregion

		public NotificationChannelProvider(ISettingsService settingsService)
			: base(settingsService)
		{
		}

		#region Overrides
		protected override string GetQuery()
		{
			var channelsContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_CHANNELS_CONTENT_ID);
			var formattersContentId = SettingsService.GetSetting(SettingsTitles.NOTIFICATION_SENDER_FORMATTERS_CONTENT_ID);

			if (channelsContentId == null || formattersContentId == null)
			{
				return null;
			}
			else
			{
				return string.Format(QueryTemplate, channelsContentId, formattersContentId);
			}
		}
		#endregion
	}
}
