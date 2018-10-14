using System;
using System.Collections.Concurrent;
using System.Linq;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Notification.Services
{
    public class NotificationChannelService : INotificationChannelService
    {
        private readonly ConcurrentDictionary<string, NotificationChannelDescriptor> _channelRepository;

        public NotificationChannelService()
        {
            _channelRepository = new ConcurrentDictionary<string, NotificationChannelDescriptor>();
        }      

        #region INotificationChannelService implementation
        public NotificationChannelDescriptor[] GetNotificationChannels()
        {
             return _channelRepository.Values.ToArray();
        }

        public void UpdateNotificationChannel(string name, int productId, DateTime lastQueued, string publicationStatus)
        {
            var channel = _channelRepository.GetOrAdd(name, new NotificationChannelDescriptor() { Name = name });

            channel.LastId = productId;
            channel.LastStatus = publicationStatus;
            channel.LastQueued = lastQueued;
            channel.LastPublished = DateTime.Now;
        }
        #endregion
    }
}
