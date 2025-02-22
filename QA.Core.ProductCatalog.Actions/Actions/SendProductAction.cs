﻿using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.QP.Services;
using QA.Core.Linq;
using NLog;
using NLog.Fluent;
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
        public new const string ResourceClass = "SendProductActionStrings";


        private readonly ISettingsService _settingsService;
        private readonly IArticleService _articleService;
        private readonly IFreezeService _freezeService;
        private readonly IConnectionProvider _provider;
        private readonly IValidationService _validationService;


        internal class Local
        {
            public IProductService ProductService { get; set; }
            public IXmlProductService XmlProductService { get; set; }
            public IQPNotificationService QpNotificationService { get; set; }

        }

        public SendProductAction(ISettingsService settingsService, IArticleService articleService, IFreezeService freezeService, IValidationService validationService, IConnectionProvider provider)
        {
            _settingsService = settingsService;
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
                productIds = DoWithLogging(
                    () => Helpers.GetAllProductIds(int.Parse(context.Parameters["site_id"]), context.ContentId, _provider.GetCustomer()), 
                    "Getting all products from content {contentId}", context.ContentId
                );

                articleIdsToCheckRelationsByContentId = new Dictionary<int, int[]>
                {
                    {context.ContentId, productIds}
                };
            }
            else
            {
                productIds = DoWithLogging(
                    () => Helpers.ExtractRegionalProductIdsFromMarketing(context.ContentItemIds, _articleService, marketingProductContentId, productsFieldName), 
                    "Getting regional product ids from marketing products content {contentId} using  field {fieldName} and ids {ids}", 
                    marketingProductContentId, productsFieldName, context.ContentItemIds
                );

                articleIdsToCheckRelationsByContentId = Helpers.GetContentIds(productIds, _provider.GetCustomer());
            }

            if (productIds.Length == 0)
            {
                return ActionTaskResult.Error(new ActionTaskResultMessage()
                {
                    ResourceClass = nameof(SendProductActionStrings),
                    ResourceName = nameof(SendProductActionStrings.NotFound),
                    Extra = string.Join(", ", context.ContentItemIds)
                }, context.ContentItemIds);
            }

            foreach (var articleIdsWithContentId in articleIdsToCheckRelationsByContentId)
            {
                var checkResult =
                    DoWithLogging(
                        () => _articleService.CheckRelationSecurity(articleIdsWithContentId.Key, articleIdsWithContentId.Value, false), 
                        "Checking relation security in content {contentId} for articles {ids}", 
                        articleIdsWithContentId.Key, articleIdsWithContentId.Value
                    );

                string idsstr = string.Join(", ", checkResult.Where(n => !n.Value));

                if (!string.IsNullOrEmpty(idsstr))
                {
                    return ActionTaskResult.Error(new ActionTaskResultMessage()
                    {
                        ResourceClass = nameof(SendProductActionStrings),
                        ResourceName = nameof(SendProductActionStrings.NoRelationAccess),
                        Extra = idsstr
                    }, context.ContentItemIds);
                    
                }
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
            var validationErrors = new ConcurrentDictionary<int, ActionTaskResult>();
            var validationErrorsSerialized = new ConcurrentDictionary<int, string>();

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


                        Article[] prodsStage = DoWithLogging(
                            () => tl.ProductService.GetProductsByIds(idsToProcess.ToArray()), 
                            "Getting products {ids}", idsToProcess.ToArray()
                        );
                        IEnumerable<string> ignoredStatuses = ignoredStatus?.Split(',') ??
                                                              Enumerable.Empty<string>().ToArray();
                        var excludedStage = prodsStage.Where(n => ignoredStatuses.Contains(n.Status)).ToArray();

                        foreach (var item in excludedStage)
                            excluded.Add(item.Id);

                        prodsStage = prodsStage.Except(excludedStage).ToArray();

                        var frozenIds = new int[0];

                        if (!skipLive)
                        {
                            var idsToCheck = prodsStage.Select(p => p.Id).ToArray();
                            frozenIds = DoWithLogging(
                                () => _freezeService.GetFrozenProductIds(idsToCheck),
                                "Getting freezing state for products {ids}", idsToCheck
                            );                
                        }

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
                            var xamlValidationErrors = DoWithLogging(
                                () => _articleService.XamlValidationById(id, true), 
                                "Validating XAML for product {id}", id
                            );
                            var validationResult = ActionTaskResult.FromRulesException(xamlValidationErrors, id);
                            
                            if (!validationResult.IsSuccess)
                            {
                                validationErrors.TryAdd(id, validationResult);
                                validationErrorsSerialized.TryAdd(id, validationResult.ToString());
                            }
                        }
                        prodsStage = prodsStage.Where(w => !validationErrors.Keys.Contains(w.Id)).ToArray();

                        var prodsLive = new Article[] { };

                        if (!skipLive)
                        {
                            if (!skipPublishing)
                            {
                                prodsLive = prodsStage;
                            }
                            else
                            {
                                prodsLive = DoWithLogging(
                                    () => tl.ProductService.GetProductsByIds(idsToProcess.ToArray(), true),
                                    "Getting separate live products {ids}", idsToProcess.ToArray()
                                );     
                            }
                        }


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
                                    .SendProductsAsync(z.ToArray(), false, context.UserName, context.UserId,
                                        localize, false, channels)
                                    .ContinueWith(y => UpdateFilteredIds(filteredInLive,
                                        y.IsFaulted ? null : y.Result, z, y.Exception, errors, failed)))
                                .Concat(ArticleFilter.DefaultFilter.Filter(prodsStage)
                                    .Section(sectionSize)
                                    .Select(z => tl.QpNotificationService.SendProductsAsync(z.ToArray(), true,
                                            context.UserName, context.UserId, localize, false, channels)
                                        .ContinueWith(y => UpdateFilteredIds(filteredInStage,
                                            y.IsFaulted ? null : y.Result, z, y.Exception, errors, failed))))
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

                            DoWithLogging(() => Task.WaitAll(tasks),
                                "Sending notifications for live ({liveIds}) and stage ({stageIds}) products",
                                ArticleFilter.LiveFilter.Filter(prodsLive).Select(n => n.Id).ToArray(),
                                ArticleFilter.DefaultFilter.Filter(prodsStage).Select(n => n.Id).ToArray()
                            );
                        }
                        else
                        {
                            lock (percentLocker)
                            {
                                currentPercent += (float)10 / parts.Length;
                            }
                            TaskContext.SetProgress((byte)currentPercent);                          
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
                                DoWithLogging(
                                    () => publishAction.Process(publishActionContext),
                                    "Calling PublishAction for products {ids}",
                                    prodsToPublishIds
                                );
                            }
                            catch (ActionException ex)
                            {

                                var ids = ex.InnerExceptions.OfType<ProductException>().Select(x => x.ProductId);
                                Logger.ForErrorEvent()
                                    .Exception(ex)
                                    .Message("Exception has been thrown while publishing products {ids}", prodsToPublishIds)
                                    .Log();                                

                                foreach (var pID in ids)
                                {
                                    failed.TryAdd(pID, null);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.ForErrorEvent()
                                    .Exception(ex)
                                    .Message("Exception has been thrown while publishing products {ids}", prodsToPublishIds)
                                    .Log();         
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
                        Logger.ForErrorEvent().Message("SendProductAction exception").Exception(ex).Log();

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
                
                return ActionTaskResult.Error(new ActionTaskResultMessage()
                {
                    ResourceClass = nameof(SendProductActionStrings),
                    ResourceName = nameof(SendProductActionStrings.Cancelled)
                }, context.ContentItemIds);      
            }

            var productsToRemove = new int[0];
            var productsToRemoveCheck = missing
                .Concat(invisibleOrArchivedIds)
                .Except(excluded)
                .Except(frozen)
                .Except(validationErrors.Keys)
                .ToArray();

            if (productsToRemoveCheck.Length > 0)
            {
                // проверяем, какие из проблемных продуктов присутствуют на витрине
                var cmService = ObjectFactoryBase.Resolve<IList<IConsumerMonitoringService>>();

                productsToRemove = DoWithLogging(
                    () => cmService.SelectMany(s => s.FindExistingProducts(productsToRemoveCheck)).Distinct().ToArray(),
                    "Checking whether products {ids} are missing on fronts", productsToRemoveCheck
                );

                if (productsToRemove.Length > 0)
                {
                    // эти продукты отсутствуют в DPC или не видны, но остались на витрине
                    // их надо удалить с витрин
                    var service = ObjectFactoryBase.Resolve<IQPNotificationService>();
                    var productService = ObjectFactoryBase.Resolve<IProductService>();
                    DoWithLogging(
                        () => Task.WhenAll(productsToRemove.Section(20).Select(
                            s => service.DeleteProductsAsync(
                                productService.GetSimpleProductsByIds(s.ToArray()), 
                                context.UserName, 
                                context.UserId, 
                                false)
                            )
                        ).Wait(),
                        "Removing missing products from fronts {ids}", productsToRemove
                    );    
                }
            }
            
            DoWithLogging(
                () => _validationService.UpdateValidationInfo(productIds, validationErrorsSerialized),
                "Updating validation info for products {ids}", productIds
            );      

            int[] notFound = missing.Except(productsToRemove).Except(excluded).Except(frozen).Except(validationErrors.Keys).ToArray();

            var notSucceeded = failed.Keys.Concat(notFound).Concat(excluded).Concat(frozen)
                .Concat(validationErrors.Keys).ToArray();

            var result = new ActionTaskResult() {FailedIds = notSucceeded};
            
            var msg = new ActionTaskResultMessage() { ResourceClass = nameof(SendProductActionStrings)};
            
            if (notSucceeded.Any())
            {
                msg.ResourceName = nameof(SendProductActionStrings.PartiallySucceededResult);
                msg.Extra = string.Join(", ", notSucceeded);
                msg.Parameters = new object[] {productIds.Length - notSucceeded.Length, productIds.Length};
            }
            
            else
            {
                msg.ResourceName = nameof(SendProductActionStrings.SucceededResult);
                msg.Parameters = new object[] {productIds.Length};
            }

            result.Messages.Add(msg);

            if (errors.Any())
            {
                result.Messages.Add( new ActionTaskResultMessage()
                {
                    ResourceClass = nameof(SendProductActionStrings),
                    ResourceName = nameof(SendProductActionStrings.Errors),
                    Extra = string.Join(", ", errors.Select(x => x.Message).Distinct())
                });
            }

            AddMessages(result, nameof(SendProductActionStrings.ExcludedByStatus), excluded.ToArray());
            AddMessages(result, nameof(SendProductActionStrings.ExcludedWithFreezing), frozen.ToArray());
            AddMessages(result, nameof(SendProductActionStrings.NotFoundInDpc), notFound.ToArray());
            AddMessages(result, nameof(SendProductActionStrings.RemovedFromFronts), productsToRemove.ToArray());
            AddMessages(result, nameof(SendProductActionStrings.NotPassedByStageFiltration), filteredInStage.ToArray());
            AddMessages(result, nameof(SendProductActionStrings.NotPassedByLiveFiltration), filteredInLive.ToArray());
            
            if (validationErrors.Any())
            {
                result.Messages.AddRange(validationErrors.SelectMany(v => v.Value.Messages));
            }

            TaskContext.Result = result;
            
            return result;
        }

        private void AddMessages(ActionTaskResult result, string key, int[] ints)
        {
            if (ints.Any())
            {
                result.Messages.Add(
                    new ActionTaskResultMessage()
                    {
                        ResourceClass = nameof(SendProductActionStrings),
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

                Logger.ForErrorEvent().Message($"Exception while sending products: {string.Join(", ", ids)}").Exception(exception).Log();
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
