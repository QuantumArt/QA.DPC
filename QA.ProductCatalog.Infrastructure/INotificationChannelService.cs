using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
    public interface INotificationChannelService
    {
        NotificationChannelDescriptor[] GetNotificationChannels();
        void UpdateNotificationChannel(string name, int productId, DateTime created, string publicationStatus);
    }
}
