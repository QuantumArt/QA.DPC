﻿
namespace QA.Core.DPC
{
	public interface INotificationService
	{
		void PushNotifications(NotificationItem[] notifications, bool isStage, int userId, string userName, string method, string customerCode);

		void UpdateConfiguration(string customerCode);

		ConfigurationInfo GetConfigurationInfo(string customerCode);

	}
}