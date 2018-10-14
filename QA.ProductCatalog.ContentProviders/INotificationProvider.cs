
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.ContentProviders
{
    public interface INotificationProvider
	{
		NotificationSenderConfig GetConfiguration();
	}
}
