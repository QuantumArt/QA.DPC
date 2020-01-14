using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;
using System.Globalization;
using QA.Core.DPC.QP.Services;
using QA.Core.Linq;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class ProductRelevanceAction : ActionTaskBase
    {
        private readonly Func<bool, CultureInfo, IConsumerMonitoringService> _consumerMonitoringServiceFunc;
        private readonly ISettingsService _settingsService;
        private readonly IArticleService _articleService;
        private readonly IConnectionProvider _provider;

        public ProductRelevanceAction(Func<bool, CultureInfo, IConsumerMonitoringService> consumerMonitoringServiceFunc, ISettingsService settingsService, IArticleService articleService, IConnectionProvider provider)
        {
            _consumerMonitoringServiceFunc = consumerMonitoringServiceFunc;
            _settingsService = settingsService;
            _articleService = articleService;
            _provider = provider;
        }

        public override ActionTaskResult Process(ActionContext context)
        {
            int marketingProductContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID));

            int[] productIds = context.ContentId == marketingProductContentId
                ? Helpers.GetProductIdsFromMarketingProducts(context.ContentItemIds, _articleService, _settingsService)
                : context.ContentItemIds;

            if (productIds == null || productIds.Length == 0)
            {
                if (context.ContentId == marketingProductContentId)
                    throw new Exception("Нельзя обрабатывать все маркетинговые продукты сразу, виберите конкретные продукты");

				productIds = Helpers.GetAllProductIds(int.Parse(context.Parameters["site_id"]), context.ContentId, _provider.GetCustomer());
            }

            object percentLocker = new object();

            float percentPerProduct = (float)100 / productIds.Length;

            float currentPercent = 0;

            const byte tasksCount = 15;

            const int minBundleSize = 15;

            int bundleSize = Math.Max(productIds.Length/tasksCount, minBundleSize); 

            var sectionedProductIds = productIds.Section(bundleSize).ToArray();

            var tasks = sectionedProductIds
                .Select(x => Task.Factory.StartNew(
                    productIdsInSection =>
                    {
                        var productRelevanceService = ObjectFactoryBase.Resolve<IProductRelevanceService>();
						var productService = ObjectFactoryBase.Resolve<IProductService>();

                        foreach (int productId in (IList<int>)productIdsInSection)
                        {                            
                            foreach (var isLive in new[] { true, false })
                            {
                                var product = productService.GetProductById(productId, isLive);
                                var relevanceItems = productRelevanceService.GetProductRelevance(product, isLive, true);

                                foreach (var relevanceItem in relevanceItems)
                                {
                                    var consumerMonitoringService = _consumerMonitoringServiceFunc(true, relevanceItem.Culture);

                                    if (consumerMonitoringService != null)
                                    {
                                        consumerMonitoringService.InsertOrUpdateProductRelevanceStatus(productId, relevanceItem.Relevance, isLive);
                                    }

                                    if (TaskContext.IsCancellationRequested)
                                    {
                                        TaskContext.IsCancelled = true;
                                        return;
                                    }
                                }
                            }

                            if (TaskContext.IsCancellationRequested)
                            {
                                TaskContext.IsCancelled = true;
                                return;
                            }

                            lock (percentLocker)
                            {
                                currentPercent += percentPerProduct;
                            }

                            TaskContext.SetProgress((byte)currentPercent);
                        }                       
                    },
                    x,
                    TaskCreationOptions.LongRunning))
                .ToArray();

            Task.WaitAll(tasks);

            return ActionTaskResult.Success($"Статусы {productIds.Length} продуктов успешно обновлены");
        }
    }
}
