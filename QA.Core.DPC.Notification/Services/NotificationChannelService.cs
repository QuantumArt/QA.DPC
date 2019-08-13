using System;
using System.Collections.Concurrent;
using System.Linq;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.DPC.Notification.Services
{
    public class NotificationChannelService : INotificationChannelService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, NotificationChannelDescriptor>> _channelRepository;

        public NotificationChannelService()
        {
            _channelRepository = new ConcurrentDictionary<string, ConcurrentDictionary<string, NotificationChannelDescriptor>>();
        }
        
        private ConcurrentDictionary<string, NotificationChannelDescriptor> GetOrAddCustomerCode(string customerCode)
        {
            return _channelRepository.GetOrAdd(customerCode,
                new ConcurrentDictionary<string, NotificationChannelDescriptor>());
        }        

        #region INotificationChannelService implementation
        public NotificationChannelDescriptor[] GetNotificationChannels(string customerCode)
        {
            return GetOrAddCustomerCode(customerCode).Values.ToArray();
        }

        public void UpdateNotificationChannel(string customerCode, string name, int productId, DateTime lastQueued, string publicationStatus)
        {
            var channel = GetOrAddCustomerCode(customerCode)
                .GetOrAdd(name, new NotificationChannelDescriptor() { Name = name });

            channel.LastId = productId;
            channel.LastStatus = publicationStatus;
            channel.LastQueued = lastQueued;
            channel.LastPublished = DateTime.Now;
        }
        #endregion
    }
}
