using QA.Core.DPC.QP.Autopublish.Models;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Linq;
using QA.Core.Logger;

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

                        _logger.LogInfo(() => $"Autopublish {items.Count()} products for channels ['{string.Join("', '", channels.Select(c => c.Name))}'] for {customerCode}");

                        foreach (var item in items)
                        {
                            try
                            {
                                if (item.PublishAction == PublishAction.Publish || item.PublishAction == PublishAction.Delete)
                                {
                                    var isStage = item.IsUnited;
                                    var method = item.PublishAction == PublishAction.Publish ? "PUT" : "DELETE";

                                    foreach (var format in formats)
                                    {
                                        var descriptor = _autopublishProvider.GetProduct(item, format.Key);

                                        if (descriptor != null)
                                        {
                                            var channelNames = format.Where(c => c.IsStage == isStage).Select(c => c.Name).ToArray();

                                            if (channelNames.Any())
                                            {
                                                _logger.LogInfo(() => $"Autopublish {(item.IsUnited ? "stage" : "live")} product {item.ProductId} by definition {item.DefinitionId} for {customerCode} and channels ['{string.Join("', '", channelNames)}']");
                                                _notificationProvider.PushNotifications(descriptor.ProductId, descriptor.Product, channelNames, isStage, 1, "Admin", method, customerCode);
                                            }
                                        }
                                    }
                                }
                                
                                _autopublishProvider.Dequeue(item);
                                _logger.LogInfo(() => $"Autopublish dequeue {(item.IsUnited ? "stage" : "live")} product {item.ProductId} by definition {item.DefinitionId} with action {item.PublishAction} for {customerCode}");
                            }
                            catch (Exception ex)
                            {
                                _logger.ErrorException($"Can't autopublish {(item.IsUnited ? "stage" : "live")} product {item.ProductId} by definition {item.DefinitionId} with action {item.PublishAction} for {customerCode}", ex);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogTrace(() => $"No product to autopublish for {customerCode}");
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
