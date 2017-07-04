
using System;

namespace QA.ProductCatalog.Infrastructure
{
	public interface INotificationProvider
	{
		NotificationSenderConfig GetConfiguration();
	}
}
