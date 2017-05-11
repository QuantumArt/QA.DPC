using QA.ProductCatalog.Infrastructure;
using System;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public class AutopublishService : ITask
    {
        private readonly IAutopublishProvider _autopublishProvider;
        private readonly INotificationProvider _notificationProvider;
        private readonly ILogger _logger;

        public AutopublishService(IAutopublishProvider autopublishProvider, INotificationProvider notificationProvider, ILogger logger)
        {
            _autopublishProvider = autopublishProvider;
            _notificationProvider = notificationProvider;
            _logger = logger;
        }

        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            var customerCode = data;
            var items = _autopublishProvider.Peek();
            var channels = new string[] { "autopublish" };

            foreach (var item in items)
            {
                try
                {
                    var descriptor = _autopublishProvider.GetProduct(item);

                    if (descriptor != null)
                    {
                        _notificationProvider.PushNotifications(descriptor.ProductId, descriptor.Product, channels, true, 1, "Admin", "PUT", customerCode);
                        _autopublishProvider.Dequeue(item);
                        _logger.LogTrace(() => $"Product {item.ProductId} was published");
                    }
                }
                catch (Exception ex)
                {
                    _logger.ErrorException($"Cant process product {item.ProductId}", ex);
                }
            }
        }
    }
}
