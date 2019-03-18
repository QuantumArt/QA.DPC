using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QA.Core;
using QA.Core.Linq;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Integration;

namespace QA.ProductCatalog.Admin.WebApp.Models
{

    public class SendProductModel
    {
        public string Message { get; set; }
        [Display(Name = "Список ID продуктов, которые следует опубликовать перед отправкой")]
        public string ArticleIds { get; set; }

        public Article[] NeedPublishing { get; set; }

        public int[] Removed { get; set; }

        public int[] NotFound { get; set; }

        [Display(Name = "Список ID продуктов")]
        public string Ids { get; set; }

        internal static Result Send(int[] ids, int bundleSize)
        {
            var parts = ids.Section(bundleSize);
            var failed = new ConcurrentBag<int>();
            var missing = new ConcurrentBag<int>();
            var invisibleOrArchived = new ConcurrentBag<int>();
            var errors = new ConcurrentBag<Exception>();
            var productsToPublish = new ConcurrentBag<Article>();

	        var userProvider = ObjectFactoryBase.Resolve<IUserProvider>();

	        int userId = userProvider.GetUserId();

	        string userName = userProvider.GetUserName();

            HttpContextUserProvider.ForcedUserId = userId;

            Parallel.ForEach(parts, new ParallelOptions { MaxDegreeOfParallelism = 12 },
                () =>
                {
                    HttpContextUserProvider.ForcedUserId = userId;
                    return new TLocal
                    {
                        ProductService = ObjectFactoryBase.Resolve<IProductService>(),
                        QPNotificationService = ObjectFactoryBase.Resolve<IQPNotificationService>(),
                    };
                },
                (x, ps, tl) =>
                {
                    try
                    {
                        int n = 0;
						Article[] ps1 = null;
						Article[] ps2 = null;


                        while (true)
                        {
                            // три попытки обработать продукт
                            try
                            {
                                var idsToPublish = new HashSet<int>();
                                var idsToRemove = new HashSet<int>();
                                var prods = tl.ProductService.GetProductsByIds(288, x.ToArray(), false);								

                                // проверим, что продукты не надо публиковать или архивировать
                                foreach (var product in prods)
                                {
                                    if (product.Archived || product.Visible == false)
                                    {
                                        // архивные или невидимые продукты следует удалить с витрин
                                        idsToRemove.Add(product.Id);
                                        continue;
                                    }

                                    if (!product.IsPublished || PublishAction.GetAllArticlesToCheck(product).Where(p => !p.IsPublished).Any())
                                    {
                                        // эти продукты имеют неопубликованные или расщепленные статьи
                                        productsToPublish.Add(product);
                                        idsToPublish.Add(product.Id);
                                    }
                                }

                                foreach (var item in x.Except(prods.Select(y => y.Id)).Except(idsToPublish))
                                {
                                    missing.Add(item);
                                }

                                // удалим требующие публикации или удаления продукты
                                prods = prods.Where(p => !idsToPublish.Contains(p.Id) || idsToRemove.Contains(p.Id)).ToArray();


                                // так как мы не отправляем продукты с неопубликовнными изменениями, то stage-версия продукта опубликована.
                                // используем один продукт для формирования двух xml
								ps1 = ArticleFilter.LiveFilter.Filter(prods).ToArray();
								ps2 = ArticleFilter.DefaultFilter.Filter(prods).ToArray();

                                foreach (var item in idsToRemove)
                                {
                                    invisibleOrArchived.Add(item);
                                }

                                break;
                            }
                            catch (Exception ex)
                            {
                                //защита от временных сбойев sql
                                Thread.Sleep(50);
                                n++;
                                if (n >= 3)
                                {
                                    errors.Add(ex);
                                    foreach (var item in x)
                                    {
                                        failed.Add(item);
                                    }
                                    return tl;
                                }
                            }
                        }

						
						var tasks =
							ps1
							.Section(Math.Min(bundleSize, 5))
							.Select(z => tl.QPNotificationService.SendProductsAsync(z.ToArray(), false, userName, userId, false, false))
							.Union
							(ps2
							.Section(Math.Min(bundleSize, 5))
							.Select(z => tl.QPNotificationService.SendProductsAsync(z.ToArray(), true, userName, userId, false, false))
							).ToList();

                        Task.WhenAll(tasks).Wait();
						
                        return tl;
                    }
                    catch (Exception ex)
                    {
                        foreach (var item in x)
                        {
                            failed.Add(item);
                        }

                        errors.Add(ex);
                    }
                    return tl;
                }, tt => { });

            var productsToRemove = missing
                .Union(invisibleOrArchived)
                .Distinct()
                .ToArray();

            if (productsToRemove.Length > 0)
            {
                // проверяем, какие из проблемных продуктов присутствуют на витрине
                productsToRemove = ObjectFactoryBase.Resolve<IConsumerMonitoringService>().FindExistingProducts(productsToRemove);

                if (productsToRemove.Length > 0)
                {
                    // эти продукты отсутствуют в DPC или не видны, но остались на витрине
                    // их надо удалить с витрин
                    var service = ObjectFactoryBase.Resolve<IQPNotificationService>();

					Task.WhenAll(productsToRemove
							.Section(20).Select(s => service.DeleteProductsAsync(s.Select(id => new Article() { Id = id }).ToArray(), userName, userId, false)))
						.Wait();
                }
            }

            var products = productsToPublish.ToArray();

            return new Result
            {
                Failed = failed.ToArray(),
                NotFound = missing.Except(productsToRemove).ToArray(),
                Removed = productsToRemove.Distinct().ToArray(),
                NeedPublishing = products,
                Errors = errors.ToArray()
            };
        }

        internal class Result
        {
            public Exception[] Errors { get; set; }
            public int[] Failed { get; set; }
            public int[] Archived { get; set; }
            public Article[] NeedPublishing { get; set; }

            public int[] Removed { get; set; }

            public int[] NotFound { get; set; }
        }

        internal class TLocal
        {
            public IProductService ProductService { get; set; }            
            public IQPNotificationService QPNotificationService { get; set; }
        }

        public SendProductModel()
        {
            NeedPublishing = new Article[] { };
        }
    }
}