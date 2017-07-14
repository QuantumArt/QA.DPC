using QA.Core.DPC.QP.Autopublish.Models;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Linq;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public class AutopublishService : ITask
    {
        private readonly IAutopublishProvider _autopublishProvider;
        private readonly INotificationAutopublishProvider _notificationProvider;
        private readonly INotificationProvider _configurationProvider;
        private readonly ILogger _logger;

        public AutopublishService(IAutopublishProvider autopublishProvider, INotificationAutopublishProvider notificationProvider, INotificationProvider configurationProvider, ILogger logger)
        {
            _autopublishProvider = autopublishProvider;
            _notificationProvider = notificationProvider;
            _configurationProvider = configurationProvider;
            _logger = logger;
        }

        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            var customerCode = data;

            try
            {
                var channels = GetChannels();

                if (channels.Any())
                {
                    var items = _autopublishProvider.Peek(customerCode);

                    if (items.Any())
                    {
                        var formats = channels.GroupBy(c => c.Format);

                        foreach (var item in items)
                        {
                            try
                            {
                                foreach(var format in formats)
                                {
                                    var descriptor = _autopublishProvider.GetProduct(item, format.Key);                                    

                                    if (descriptor != null)
                                    {
                                        var channelNames = format.Select(c => c.Name).ToArray();
                                        _logger.LogInfo(() => $"Autopublish product {item.ProductId} for {customerCode} and channels ['{string.Join("', '", channelNames)}']");
                                        _notificationProvider.PushNotifications(descriptor.ProductId, descriptor.Product, channelNames, true, 1, "Admin", "PUT", customerCode);
                                        _autopublishProvider.Dequeue(item);
                                    }
                                }                               
                            }
                            catch (Exception ex)
                            {
                                _logger.ErrorException($"Can't autopublish product {item.ProductId}", ex);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.ErrorException($"Can't run autopublish for {customerCode}", ex);
            }
        }

        private NotificationChannel[] GetChannels()
        {
            return _configurationProvider
                .GetConfiguration()
                .Channels
                .Where(c => c.Autopublish)               
                .ToArray();
        }     
    }
}
