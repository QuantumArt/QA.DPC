using System;

namespace QA.ProductCatalog.ContentProviders
{
    public interface INotificationChannelService
    {
        NotificationChannelDescriptor[] GetNotificationChannels();
        void UpdateNotificationChannel(string name, int productId, DateTime created, string publicationStatus);
    }
}