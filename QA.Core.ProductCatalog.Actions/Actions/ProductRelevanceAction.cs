using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class ProductRelevanceAction : ActionTaskBase
    {
        private readonly IConsumerMonitoringService _consumerMonitoringService;

        private readonly ISettingsService _settingsService;
        private readonly IArticleService _articleService;

        public ProductRelevanceAction(IConsumerMonitoringService consumerMonitoringService, ISettingsService settingsService, IArticleService articleService)
        {
            _consumerMonitoringService = consumerMonitoringService;

            _settingsService = settingsService;

            _articleService = articleService;
        }


        public override string Process(ActionContext context)
        {
            int marketingProductContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID));

            int[] productIds = context.ContentId == marketingProductContentId
                ? Helpers.GetProductIdsFromMarketingProducts(context.ContentItemIds, _articleService, _settingsService)
                : context.ContentItemIds;

            if (productIds == null || productIds.Length == 0)
            {
                if (context.ContentId == marketingProductContentId)
                    throw new Exception("Нельзя обрабатывать все маркетинговые продукты сразу, виберите конкретные продукты");

				productIds = Helpers.GetAllProductIds(int.Parse(context.Parameters["site_id"]), context.ContentId);
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

                        foreach (int productId in (IList<int>) productIdsInSection)
                        {
							var productStage = productService.GetProductById(productId);

	                        DateTime? trash1;
	                        string trash2;

							var productRelevanceStage = productRelevanceService.GetProductRelevance(productStage, false, out trash1, out trash2);

	                        var productLive = productStage.Splitted ? productService.GetProductById(productId, true): productStage;

							var productRelevanceLive = productRelevanceService.GetProductRelevance(productLive, true, out trash1, out trash2);

							_consumerMonitoringService.InsertOrUpdateProductRelevanceStatus(productId, productRelevanceLive, true);

							_consumerMonitoringService.InsertOrUpdateProductRelevanceStatus(productId, productRelevanceStage, false);

                            if (TaskContext.IsCancellationRequested)
                            {
                                TaskContext.IsCancelled = true;

                                return;
                            }

                            lock (percentLocker)
                            {
                                currentPercent += percentPerProduct;
                            }

                            TaskContext.SetProgress((byte) currentPercent);
                        }
                    },
                    x,
                    TaskCreationOptions.LongRunning))
                .ToArray();

            Task.WaitAll(tasks);

            return string.Format("Статусы {0} продуктов успешно обновлены", productIds.Length);
        }
    }
}
