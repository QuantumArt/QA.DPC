using System.ServiceModel;

namespace QA.Core.DPC
{
	[ServiceContract]
	public interface INotificationService
	{
		[OperationContract]
		void PushNotifications(NotificationItem[] notifications, bool isStage, int userId, string userName, string method, string customerCode);

		[OperationContract]
		void UpdateConfiguration(string customerCode);

		[OperationContract]
		ConfigurationInfo GetConfigurationInfo(string customerCode);

	}
}