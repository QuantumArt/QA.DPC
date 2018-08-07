using System;
using System.Linq;
using System.Threading.Tasks;
using QA.Core.DPC.Formatters.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.Core.Models.Processors;
using QA.Core.Service.Interaction;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration.Notifications;

namespace QA.ProductCatalog.Integration
{
    // ReSharper disable once InconsistentNaming
    public class QPNotificationService : QAClientBase, IQPNotificationService
    {
        private const string Put = "PUT";
        private const string Delete = "DELETE";

        private readonly IContentProvider<NotificationChannel> _channelProvider;
        private readonly Func<string, IArticleFormatter> _getFormatter;
        private readonly IProductLocalizationService _localizationService;
        private readonly IIdentityProvider _identityProvider;

        public QPNotificationService(IContentProvider<NotificationChannel> channelProvider, Func<string, IArticleFormatter> getFormatter, IProductLocalizationService localizationService, IIdentityProvider identityProvider)
        {
            _channelProvider = channelProvider;
            _getFormatter = getFormatter;
            _localizationService = localizationService;
            _identityProvider = identityProvider;
        }

        protected override void OnInitializeClient(object service)
        {
        }

        public async Task<int[]> SendProductsAsync(Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null)
        {
            return await PushProductsAsync(products, isStage, userName, userId, Put, localize, autopublish, forcedСhannels);
        }

        public int[] SendProducts(Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null)
        {
            return PushProducts(products, isStage, userName, userId, Put, localize, autopublish, forcedСhannels);
        }

        public async Task DeleteProductsAsync(Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null)
        {
            await PushProductsAsync(products, true, userName, userId, Delete, false, autopublish, forcedСhannels);
            await PushProductsAsync(products, false, userName, userId, Delete, false, autopublish, forcedСhannels);
        }

        public void DeleteProducts(Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null)
        {
            PushProducts(products, true, userName, userId, Delete, false, autopublish, forcedСhannels);
            PushProducts(products, false, userName, userId, Delete, false, autopublish, forcedСhannels);
        }

        public async Task UnpublishProductsAsync(Article[] products, string userName, int userId, string[] forcedСhannels = null)
        {
            await PushProductsAsync(products, false, userName, userId, Delete, false, false, forcedСhannels);
        }

        public void UnpublishProducts(Article[] products, string userName, int userId, string[] forcedСhannels = null)
        {
            PushProducts(products, false, userName, userId, Delete, false, false, forcedСhannels);
        }

        private string Convert(Article product, string formatter)
        {
            return _getFormatter(formatter).Serialize(product, ArticleFilter.DefaultFilter, true);
        }

        private int[] PushProducts(Article[] products, bool isStage, string userName, int userId, string method, bool localize, bool autopublish, string[] forcedСhannels)
        {
            var service = new NotificationServiceClient();
            var notifications = GetNotifications(products, isStage, forcedСhannels, localize, autopublish);
            if (notifications == null)
            {
                return null;
            }

            if (notifications.Any())
            {
                var customerCode = _identityProvider.Identity.CustomerCode;
                service.PushNotifications(notifications, isStage, userId, userName, method, customerCode);
            }

            return notifications.Select(n => n.ProductId).ToArray();
        }

        private async Task<int[]> PushProductsAsync(Article[] products, bool isStage, string userName, int userId, string method, bool localize, bool autopublish, string[] forcedСhannels)
        {
            var service = new NotificationServiceClient();
            var notifications = GetNotifications(products, isStage, forcedСhannels, localize, autopublish);

            if (notifications == null)
            {
                return null;
            }
            if (notifications.Any())
            {
                var customerCode = _identityProvider.Identity.CustomerCode;
                await service.PushNotificationsAsync(notifications, isStage, userId, userName, method, customerCode);
            }

            return notifications.Select(n => n.ProductId).ToArray();
        }

        private NotificationItem[] GetNotifications(Article[] products, bool isStage, string[] forcedСhannels, bool localize, bool autopublish)
        {
            var channels = _channelProvider.GetArticles();
            NotificationItem[] notifications = null;

            if (channels != null && forcedСhannels != null)
            {
                channels = channels.Where(c => forcedСhannels.Contains(c.Name)).ToArray();
            }

            if (channels == null)
            {
                notifications =
                    (from p in products
                     select
                    new NotificationItem
                    {
                        Channels = null,
                        Data = Convert(p, typeof(XmlProductFormatter).Name),
                        ProductId = p.Id
                    }).ToArray();

            }
            else if (channels.Any(c => c.Autopublish == autopublish))
            {
                channels = channels.Where(c => c.IsStage == isStage && c.Autopublish == autopublish).ToArray();

                if (channels.Any())
                {

                    if (localize)
                    {
                        notifications = (from items in
                                             from c in channels
                                             from p in products
                                             where Match(c.Filter, p)
                                             group c by p into g
                                             let cultures = g.Select(x => x.Culture).Distinct().ToArray()
                                             let localizationMap = _localizationService.SplitLocalizations(g.Key, cultures, true)
                                             select from c2 in g
                                                    group c2 by new { c2.Culture, c2.Formatter } into g2
                                                    let localProduct = localizationMap[g2.Key.Culture]
                                                    select new NotificationItem
                                                    {
                                                        Channels = g2.Select(x => x.Name).ToArray(),
                                                        Data = Convert(localProduct, g2.Key.Formatter),
                                                        ProductId = localProduct.Id
                                                    }
                                         from item in items
                                         select item)
                                         .ToArray();
                    }
                    else
                    {
                        notifications = (from c in channels
                                         from p in products
                                         where Match(c.Filter, p)
                                         group new { Channel = c, Product = p } by new { Product = p, c.Formatter } into g
                                         select new NotificationItem
                                         {
                                             Channels = g.Select(x => x.Channel.Name).ToArray(),
                                             Data = Convert(g.Key.Product, g.Key.Formatter),
                                             ProductId = g.Key.Product.Id
                                         })
                                       .ToArray();
                    }
                }
            }
            else if (!autopublish)
            {
                throw new Exception("Не найдено каналов для публикации");
            }

            return notifications;
        }

        private static bool Match(string filter, Article product)
        {
            return string.IsNullOrEmpty(filter) || DPathProcessor.Process(filter, product).Any();
        }
    }
}