using QA.Core.DPC.QP.Autopublish.Models;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace QA.Core.DPC.QP.Autopublish.Services
{
    public class AutopublishService : ITask
    {
        private readonly IAutopublishProvider _autopublishProvider;
        private readonly INotificationProvider _configurationProvider;
        private readonly ILogger _logger;

        public AutopublishService(IAutopublishProvider autopublishProvider, INotificationProvider configurationProvider, ILogger logger)
        {
            _autopublishProvider = autopublishProvider;
            _configurationProvider = configurationProvider;
            _logger = logger;
        }

        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            var customerCode = data;

            try
            {
                var items = _autopublishProvider.Peek(customerCode);

                if (items.Any())
                {
                    _logger.LogInfo(() => $"Autopublish {items.Count()} products for {customerCode}");

                    foreach (var item in items)
                    {
                        try
                        {
                            if (item.PublishAction == PublishAction.Publish || item.PublishAction == PublishAction.Delete)
                            {
                                _autopublishProvider.PublishProduct(item);
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
            catch(Exception ex)
            {
                _logger.ErrorException($"Can't run autopublish for {customerCode}", ex);
            }
        }
    }
}
