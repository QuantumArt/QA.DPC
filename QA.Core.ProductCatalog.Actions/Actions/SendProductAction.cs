using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Linq;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;
using System;
using QA.ProductCatalog.ContentProviders;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Resources;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class SendProductAction : ActionTaskBase
    {
        private const int DefaultBundleSize = 15;
        private const int DefaultMaxDegreeOfParallelism = 12;
        public const string ResourceClass = "SendProductActionStrings";


        private readonly ISettingsService _settingsService;
        private readonly IArticleService _articleService;
        private readonly ILogger _logger;
        private readonly IFreezeService _freezeService;
        private readonly IConnectionProvider _provider;
        private readonly IValidationService _validationService;


        internal class Local
        {
            public IProductService ProductService { get; set; }
            public IXmlProductService XmlProductService { get; set; }
            public IQPNotificationService QpNotificationService { get; set; }

        }

        public SendProductAction(ISettingsService settingsService, IArticleService articleService, ILogger logger, IFreezeService freezeService, IValidationService validationService, IConnectionProvider provider)
        {
            _settingsService = settingsService;
            _logger = logger;
            _articleService = articleService;
            _freezeService = freezeService;
            _validationService = validationService;
            _provider = provider;
        }

        public override ActionTaskResult Process(ActionContext context)
        {
            int bundleSize = GetBundleSize();
            int maxDegreeOfParallelism = GetMaxDegreeOfParallelism();

            string[] channels = context.Parameters.GetChannels();
            bool localize = context.Parameters.GetLocalize();

            int marketingProductContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_CONTENT_ID));
            string productsFieldName = _settingsService.GetSetting(SettingsTitles.MARKETING_PRODUCT_PRODUCTS_FIELD_NAME);

            Dictionary<int, int[]> articleIdsToCheckRelationsByContentId;

            int[] productIds;

            if (context.ContentItemIds == null || context.ContentItemIds.Length == 0)
            {
                productIds = Helpers.GetAllProductIds(int.Parse(context.Parameters["site_id"]), context.ContentId, _provider.GetCustomer());

                articleIdsToCheckRelationsByContentId = new Dictionary<int, int[]>
                {
                    {context.ContentId, productIds}
                };
            }
            else
            {
                productIds = Helpers.ExtractRegionalProductIdsFromMarketing(context.ContentItemIds, _articleService, marketingProductContentId, productsFieldName);

                articleIdsToCheckRelationsByContentId = Helpers.GetContentIds(productIds, _provider.GetCustomer());
            }

            if (productIds.Length == 0)
                throw new Exception(SendProductActionStrings.NotFound);

            foreach (var articleIdsWithContentId in articleIdsToCheckRelationsByContentId)
            {
                var checkResult = _articleService.CheckRelationSecurity(articleIdsWithContentId.Key, articleIdsWithContentId.Value, false);

                string idsstr = string.Join(", ", checkResult.Where(n => !n.Value));

                if (!string.IsNullOrEmpty(idsstr))
                    throw new Exception($"{SendProductActionStrings.NoRelationAccess} {idsstr}");
            }

            const string skipPublishingKey = "skipPublishing";

            bool skipPublishing = context.Parameters.ContainsKey(skipPublishingKey) && bool.Parse(context.Parameters[skipPublishingKey]);

            const string skipLiveKey = "skipLive";

            bool skipLive = context.Parameters.ContainsKey(skipLiveKey) && bool.Parse(context.Parameters[skipLiveKey]);

            const string ignoredStatusKey = "IgnoredStatus";

            string ignoredStatus = (context.Parameters.ContainsKey(ignoredStatusKey)) ? context.Parameters[ignoredStatusKey] : null;


            float currentPercent = 0;

            object percentLocker = new object();



            var parts = productIds.Section(bundleSize).ToArray();

            var filteredInStage = new ConcurrentBag<int>();
            var filteredInLive = new ConcurrentBag<int>();
            var failed = new ConcurrentDictionary<int, object>();
            var missing = new ConcurrentBag<int>();
            var excluded = new ConcurrentBag<int>();
            var frozen = new ConcurrentBag<int>();
            var invisibleOrArchivedIds = new ConcurrentBag<int>();
            var errors = new ConcurrentBag<Exception>();
            var validationErrors = new ConcurrentDictionary<int, string>();

            Parallel.ForEach(parts, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
                () =>
                {
                    HttpContextUserProvider.ForcedUserId = context.UserId;
                    return new Local
                    {
                        ProductService = ObjectFactoryBase.Resolve<IProductService>(),
                        QpNotificationService = ObjectFactoryBase.Resolve<IQPNotificationService>(),
                        XmlProductService = ObjectFactoryBase.Resolve<IXmlProductService>()
                    };
                },
                (idsToProcess, ps, tl) =>
                {
                    try
                    {
                        if (TaskContext.IsCancellationRequested)
                        {
                            TaskContext.IsCancelled = true;

                            return tl;
                        }

                        var localInvisibleOrArchivedIds = new HashSet<int>();


                        Article[] prodsStage = tl.ProductService.GetProductsByIds(idsToProcess.ToArray());
                        IEnumerable<string> ignoredStatuses = ignoredStatus?.Split(',') ??
                                                              Enumerable.Empty<string>().ToArray();
                        var excludedStage = prodsStage.Where(n => ignoredStatuses.Contains(n.Status)).ToArray();

                        foreach (var item in excludedStage)
                            excluded.Add(item.Id);

                        prodsStage = prodsStage.Except(excludedStage).ToArray();


                        var frozenIds = skipLive
                            ? new int[0]
                            : _freezeService.GetFrozenProductIds(prodsStage.Select(p => p.Id).ToArray());

                        prodsStage = prodsStage.Where(p => !frozenIds.Contains(p.Id)).ToArray();

                        foreach (int id in frozenIds)
                            frozen.Add(id);

                        if (TaskContext.IsCancellationRequested)
                        {
                            TaskContext.IsCancelled = true;

                            return tl;
                        }
                        
                        //Валидация продуктов
                        foreach (int id in prodsStage.Where(w => !w.Archived && w.Visible).Select(s => s.Id))
                        {
                            var xamlValidationErrors = _articleService.XamlValidationById(id, true);
                            if (!xamlValidationErrors.IsEmpty)
                            {
                                validationErrors.TryAdd(id, string.Join(Environment.NewLine,
                                    xamlValidationErrors.Errors.Select(s => s.Message)
                                ));
                            }
                        }
                        prodsStage = prodsStage.Where(w => !validationErrors.Keys.Contains(w.Id)).ToArray();


                        //если будем автоматом публиковать то нет смысла отдельно получать prodsLive, prodsStage присвоим
                        var prodsLive = skipLive
                            ? new Article[] { }
                            : skipPublishing
                                ? tl.ProductService.GetProductsByIds(idsToProcess.ToArray(), true)
                                : prodsStage;


                        if (TaskContext.IsCancellationRequested)
                        {
                            TaskContext.IsCancelled = true;

                            return tl;
                        }

                        lock (percentLocker)
                        {
                            currentPercent += (float)50 / parts.Length;
                        }

                        TaskContext.SetProgress((byte)currentPercent);

                        // архивные или невидимые продукты следует удалить с витрин
                        foreach (var product in prodsLive.Where(product => product.Archived || !product.Visible))
                            localInvisibleOrArchivedIds.Add(product.Id);

                        if (!skipLive)
                            foreach (var item in idsToProcess.Except(prodsLive.Select(y => y.Id)))
                                missing.Add(item);

                        //неопубликованные или расщепленные публикуем сразу
                        int[] prodsToPublishIds = null;

                        if (!skipPublishing)
                        {
                            prodsToPublishIds = prodsLive
                                .Where(x =>
                                    !x.Archived &&
                                    x.Visible &&
                                    (
                                        !x.IsPublished ||
                                        PublishAction.GetAllArticlesToCheck(x).Any(p => !p.IsPublished) ||
                                        _freezeService.GetFreezeState(x.Id) == FreezeState.Unfrosen
                                    )
                                )
                                .Select(x => x.Id)
                                .ToArray();

                            if (TaskContext.IsCancellationRequested)
                            {
                                TaskContext.IsCancelled = true;

                                return tl;
                            }

                            // удалим требующие публикации или удаления продукты
                            prodsLive = prodsLive
                                .Where(p => !prodsToPublishIds.Contains(p.Id) && !localInvisibleOrArchivedIds.Contains(p.Id))
                                .ToArray();
                        }


                        if (TaskContext.IsCancellationRequested)
                        {
                            TaskContext.IsCancelled = true;

                            return tl;
                        }

                        lock (percentLocker)
                        {
                            currentPercent += (float)30 / parts.Length;
                        }

                        TaskContext.SetProgress((byte)currentPercent);

                        foreach (var id in localInvisibleOrArchivedIds)
                            invisibleOrArchivedIds.Add(id);

                        int sectionSize = Math.Min(bundleSize, 5);

                        var tasks =
                                ArticleFilter.LiveFilter.Filter(prodsLive)
                                .Section(sectionSize)
                                .Select(z => tl.QpNotificationService
                                    .SendProductsAsync(z.ToArray(), false, context.UserName, context.UserId, localize, false, channels)
                                    .ContinueWith(y => UpdateFilteredIds(filteredInLive, y.IsFaulted ? null : y.Result, z, y.Exception, errors, failed)))
                                .Concat(ArticleFilter.DefaultFilter.Filter(prodsStage)
                                    .Section(sectionSize)
                                    .Select(z => tl.QpNotificationService.SendProductsAsync(z.ToArray(), true, context.UserName, context.UserId, localize, false, channels)
                                    .ContinueWith(y => UpdateFilteredIds(filteredInStage, y.IsFaulted ? null : y.Result, z, y.Exception, errors, failed))))
                                .ToArray();

                        if (tasks.Length > 0)
                        {
                            float percentsPerTask = (float)10 / parts.Length / tasks.Length;

                            tasks = tasks
                                .Select(x => x.ContinueWith(y =>
                                {
                                    lock (percentLocker)
                                    {
                                        currentPercent += percentsPerTask;
                                    }

                                    TaskContext.SetProgress((byte)currentPercent);
                                }))
                                .ToArray();

                            Task.WaitAll(tasks);
                        }

                        if (TaskContext.IsCancellationRequested)
                        {
                            TaskContext.IsCancelled = true;

                            return tl;
                        }

                        // эти продукты имеют неопубликованные или расщепленные статьи
                        if (!skipPublishing && prodsToPublishIds.Length > 0)
                        {
                            var publishAction = ObjectFactoryBase.Resolve<PublishAction>();

                            var publishActionContext = new ActionContext
                            {
                                ContentItemIds = prodsToPublishIds,
                                Parameters = new Dictionary<string, string>() { { ignoredStatusKey, ignoredStatus } },
                                UserId = context.UserId,
                                UserName = context.UserName
                            };

                            try
                            {
                                publishAction.Process(publishActionContext);
                            }
                            catch (ActionException ex)
                            {

                                var ids = ex.InnerExceptions.OfType<ProductException>().Select(x => x.ProductId);
                                _logger.ErrorException("ActionException when publish " + string.Join(",", ids), ex);

                                foreach (var pID in ids)
                                {
                                    failed.TryAdd(pID, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.ErrorException("Exception when publish ", ex);
                            }
                        }

                        lock (percentLocker)
                        {
                            currentPercent += (float)10 / parts.Length;
                        }

                        TaskContext.SetProgress((byte)currentPercent);

                        return tl;
                    }
                    catch (Exception ex)
                    {
                        _logger.ErrorException("Exception when send ", ex);

                        foreach (var item in idsToProcess)
                            failed.TryAdd(item, null);

                        errors.Add(ex);
                    }
                    
                    HttpContextUserProvider.ForcedUserId = 0;
                    return tl;
                }, tt => { });

            if (TaskContext.IsCancellationRequested)
            {
                TaskContext.IsCancelled = true;

                return ActionTaskResult.Error(SendProductActionStrings.Cancelled);
            }

            var productsToRemove = missing
                .Concat(invisibleOrArchivedIds)
                .Except(excluded)
                .Except(frozen)
                .ToArray();

            if (productsToRemove.Length > 0)
            {
                // проверяем, какие из проблемных продуктов присутствуют на витрине
                productsToRemove = ObjectFactoryBase
                    .Resolve<IList<IConsumerMonitoringService>>()
                    .SelectMany(s => s.FindExistingProducts(productsToRemove))
                    .Distinct()
                    .ToArray();

                if (productsToRemove.Length > 0)
                {
                    // эти продукты отсутствуют в DPC или не видны, но остались на витрине
                    // их надо удалить с витрин
                    var service = ObjectFactoryBase.Resolve<IQPNotificationService>();
                    var productService = ObjectFactoryBase.Resolve<IProductService>();
                    Task.WhenAll(productsToRemove.Section(20).Select(s => service.DeleteProductsAsync(productService.GetSimpleProductsByIds(s.ToArray()), context.UserName, context.UserId, false))).Wait();
                }
            }


            _validationService.UpdateValidationInfo(productIds, validationErrors);

            int[] notFound = missing.Except(productsToRemove).Except(excluded).Except(frozen).Except(validationErrors.Keys).ToArray();

            var writeErrorToLog = false;
            var notSucceeded = failed.Keys.Concat(notFound).Concat(excluded).Concat(frozen)
                .Concat(validationErrors.Keys).ToArray();

            var msg = new ActionTaskResultMessage() { ResourceClass = ResourceClass};
            
            if (notSucceeded.Any())
            {
                msg.ResourceName = "PartiallySucceededResult";
                msg.Extra = string.Join(", ", notSucceeded);
                msg.Parameters = new object[] {productIds.Length - notSucceeded.Length, productIds.Length};
            }
            
            else
            {
                msg.ResourceName = "SucceededResult";
                msg.Parameters = new object[] {productIds.Length};
            }

            var result = notSucceeded.Any() ? ActionTaskResult.PartialSuccess(msg, notSucceeded) : ActionTaskResult.Success(msg); 

            if (errors.Any())
            {
                result.Messages.Add( new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = "Errors",
                    Extra = string.Join(", ", errors.Select(x => x.Message).Distinct())
                });
            }

            AddMessages(result, ResourceClass, "ExcludedByStatus", excluded.ToArray());
            AddMessages(result, ResourceClass, "ExcludedWithFreezing", frozen.ToArray());
            AddMessages(result, ResourceClass, "NotFoundInDpc", notFound.ToArray());
            AddMessages(result, ResourceClass, "RemovedFromFronts", productsToRemove.ToArray());
            AddMessages(result, ResourceClass, "NotPassedByStageFiltration", filteredInStage.ToArray());
            AddMessages(result, ResourceClass, "NotPassedByLiveFiltration", filteredInLive.ToArray());
            
            if (validationErrors.Any())
            {
                writeErrorToLog = true;

                result.Messages.Add( new ActionTaskResultMessage()
                {
                    ResourceClass = ResourceClass,
                    ResourceName = "NotValidated",
                    Extra = string.Join(", ", validationErrors.Keys) 
                            + Environment.NewLine 
                            + string.Join("; ", validationErrors.Values)
                });
            }

            if (writeErrorToLog)
            {
                _logger.Error(result.ToString());
            }
            
            TaskContext.Result = result;
            
            return result;
        }

        private void AddMessages(ActionTaskResult result, string resource, string key, int[] ints)
        {
            if (ints.Any())
            {
                result.Messages.Add(
                    new ActionTaskResultMessage()
                    {
                        ResourceClass = resource,
                        ResourceName = key,
                        Extra = string.Join(", ", ints)
                    });
            }
        }

        private void UpdateFilteredIds(ConcurrentBag<int> filteredIds, int[] sendedIds, IList<Article> articles, Exception exception, ConcurrentBag<Exception> errors, ConcurrentDictionary<int, object> failedIds)
        {
            if (exception == null && sendedIds != null)
            {
                foreach (var id in articles.Select(a => a.Id).Except(sendedIds))
                {
                    filteredIds.Add(id);
                }
            }
            else if (exception != null)
            {
                var ids = articles.Select(a => a.Id).ToArray();

                _logger.ErrorException(string.Format("Exception while sending products: {0}", string.Join(", ", ids)), exception);

                errors.Add(exception.InnerException == null ? exception : exception.InnerException);

                foreach (var id in ids)
                {
                    failedIds.TryAdd(id, null);
                }
            }
        }

        private int GetBundleSize()
        {
            var setting = _settingsService.GetSetting(SettingsTitles.PUBLISH_BUNDLE_SIZE);
            int bundleSize;

            if (!string.IsNullOrEmpty(setting) && int.TryParse(setting, out bundleSize))
            {
                return bundleSize;
            }
            else
            {
                return DefaultBundleSize;
            }
        }

        private int GetMaxDegreeOfParallelism()
        {
            var setting = _settingsService.GetSetting(SettingsTitles.PUBLISH_DEGREE_OF_PARALLELISM);
            int maxDegreeOfParallelism;

            if (!string.IsNullOrEmpty(setting) && int.TryParse(setting, out maxDegreeOfParallelism))
            {
                return maxDegreeOfParallelism;
            }
            else
            {
                return DefaultMaxDegreeOfParallelism;
            }
        }

    }
}
