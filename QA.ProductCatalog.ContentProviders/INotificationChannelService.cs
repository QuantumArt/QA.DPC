using System;

namespace QA.ProductCatalog.ContentProviders
{
    public interface INotificationChannelService
    {
        NotificationChannelDescriptor[] GetNotificationChannels(string customerCode);
        void UpdateNotificationChannel(string customerCode, string name, int productId, DateTime created, string publicationStatus);
    }
}