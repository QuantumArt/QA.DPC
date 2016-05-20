using System.ServiceModel;

namespace QA.Core.DPC
{
	[ServiceContract]
	public interface INotificationService
	{
		[OperationContract]
		void PushNotifications(NotificationItem[] notifications, bool isStage, int userId, string userName, string method);

		[OperationContract]
		void UpdateConfiguration();

		[OperationContract]
		ConfigurationInfo GetConfigurationInfo();

	}
}