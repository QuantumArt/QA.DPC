using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.ProductCatalog.Infrastructure
{
    public interface INotificationChannelService
    {
        NotificationChannel[] GetNotificationChannels();
        void UpdateNotificationChannel(string name, int productId, string publicationStatus);
    }
}
