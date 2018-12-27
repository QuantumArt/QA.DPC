using QA.Configuration;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Notification.Services
{
    public class NotificationConfigurationProvider : INotificationProvider
	{
		private readonly IConfigurationService _configurationService;

		public NotificationConfigurationProvider(IConfigurationService configurationService)
		{
			_configurationService = configurationService;
        }

        #region INotificationProvider implementation
        public NotificationSenderConfig GetConfiguration()
		{
			return _configurationService.GetConfiguration<NotificationSenderConfig>("Notifications");
		}

	    #endregion
	}
}
