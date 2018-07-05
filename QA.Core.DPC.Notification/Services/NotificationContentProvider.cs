using System.Linq;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Notification.Services
{
    public class NotificationContentProvider : INotificationProvider
	{
		private readonly ISettingsService _settingsService;
		private readonly IContentProvider<NotificationChannel> _notificationChannelProvider;

		public NotificationContentProvider(ISettingsService settingsService, IContentProvider<NotificationChannel> notificationChannelProvider)
		{
			_settingsService = settingsService;
			_notificationChannelProvider = notificationChannelProvider;
		}

		#region INotificationProvider implementation
		public NotificationSenderConfig GetConfiguration()
		{
			return new NotificationSenderConfig
			{
				CheckInterval = GetIntValue(SettingsTitles.NOTIFICATION_SENDER_CHECK_INTERVAL),
				ErrorCountBeforeWait = GetIntValue(SettingsTitles.NOTIFICATION_SENDER_ERROR_COUNT_BERORE_WAIT),
				PackageSize = GetIntValue(SettingsTitles.NOTIFICATION_SENDER_PACKAGE_SIZE),
				TimeOut = GetIntValue(SettingsTitles.NOTIFICATION_SENDER_TIMEOUT),
				WaitIntervalAfterErrors = GetIntValue(SettingsTitles.NOTIFICATION_SENDER_WAIT_INTERVAL_AFTER_ERRORS),
				Channels = (_notificationChannelProvider.GetArticles() ?? new NotificationChannel[0]).ToList(),
                Autopublish = GetBoolValue(SettingsTitles.NOTIFICATION_SENDER_AUTOPUBLISH, false)
            };
		}

	    #endregion

		#region Private methods
		private int GetIntValue(SettingsTitles title)
		{
			return int.Parse(_settingsService.GetSetting(title));
		}

        private bool GetBoolValue(SettingsTitles title, bool defaultValue)
        {
            var value = _settingsService.GetSetting(title);

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
    }
}
