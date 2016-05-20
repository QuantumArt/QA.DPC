using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.Models;
using Quantumart.QP8.BLL.Services.DTO;

namespace QA.Core.ProductCatalog.Actions
{
    public class PublishAction : ActionBase
    {
        protected IQPNotificationService NotificationService { get; private set; }
        protected IXmlProductService XmlProductService { get; private set; }
        protected IFreezeService FreezeService { get; private set; }

        public PublishAction(
            IArticleService articleService,
            IFieldService fieldService,
            IProductService productservice,
            ILogger logger,
            Func<ITransaction> createTransaction,
            IQPNotificationService notificationService,
            IXmlProductService xmlProductService,
            IFreezeService freezeService)
            : base(articleService, fieldService, productservice, logger, createTransaction)
        {
            NotificationService = notificationService;
            XmlProductService = xmlProductService;
            FreezeService = freezeService;
        }

        #region Overrides
        protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
        {
            var transactionId = Guid.NewGuid().ToString();

			string[] channels = actionParameters.GetChannels();

			string ignoredStatus = (actionParameters.ContainsKey("IgnoredStatus")) ? actionParameters["IgnoredStatus"] : null;
			string[] ignoredStatuses = (ignoredStatus == null) ? Enumerable.Empty<string>().ToArray() : ignoredStatus.Split(new[]{','});

            var product = DoWithLogging("Productservice.GetProductById", transactionId, () => Productservice.GetProductById(productId, false));

			if (ignoredStatuses.Contains(product.Status))
				ValidateMessageResult(product.Id, MessageResult.Error("продукт исключен по статусу"));

			if (!ArticleFilter.DefaultFilter.Matches(product))
				ValidateMessageResult(product.Id, MessageResult.Error("продукт не подлежит публикации"));

            var state = FreezeService.GetFreezeState(product.Id);

            if (state == FreezeState.Frozen)
            {
                ValidateMessageResult(product.Id, MessageResult.Error("продукт заморожен"));
            }

			var allArticles = GetAllArticles(new[] { product });
			bool containsIgnored = allArticles.Any(a => ignoredStatuses.Contains(a.Status));

            var articleIds = DoWithLogging("GetAllArticles", transactionId, () => allArticles.Where(a => a.Id != productId && !a.IsPublished && !ignoredStatuses.Contains(a.Status)).Select(a => a.Id).Distinct().ToArray());

            var result = DoWithLogging("ArticleService.Publish", transactionId, () => ArticleService.Publish(product.ContentId, new[] { productId }));
            ValidateMessageResult(productId, result);

            DoWithLogging("ArticleService.SimplePublish", transactionId, () => ArticleService.SimplePublish(articleIds));

            if (state == FreezeState.Unfrosen)
            {
                FreezeService.ResetFreezing(product.Id);
            }

            const string doNotSendNotificationsKey = "DoNotSendNotifications";
			bool doNotSendNotifications = actionParameters.ContainsKey(doNotSendNotificationsKey) && bool.Parse(actionParameters[doNotSendNotificationsKey]);

	        if (!doNotSendNotifications)
			{
				DoWithLogging("SendNotification (includes substeps)", transactionId, () => SendNotification(product, transactionId, UserName, UserId, containsIgnored, channels));
            }          
        }
        #endregion

        #region Private methods

        public static IEnumerable<Article> GetAllArticlesToCheck(params Article[] articles)
        {
            return GetAllArticles(articles);
        }

      	private static IEnumerable<Article> GetAllArticles(IEnumerable<Article> articles, bool isExsteinsion = false)
		{
			foreach (var article in articles)
			{
				if (article != null && article.PublishingMode == PublishingMode.Publish)
				{
					if (!isExsteinsion)
					{
						yield return article;
					}

					var referencedArticles = new List<Article>();

					foreach (var field in article.Fields.Values.OfType<SingleArticleField>())
					{
						if (field.Item != null)
						{
							referencedArticles.AddRange(GetAllArticles(new[] { field.Item }, field is ExtensionArticleField));
						}
					}

					foreach (var field in article.Fields.Values.OfType<MultiArticleField>())
					{
						referencedArticles.AddRange(GetAllArticles(field));
					}

					foreach (var referencedArticle in referencedArticles)
					{
						if (referencedArticle != null)
						{
							yield return referencedArticle;
						}
					}
				}
			}
		}

		private void SendNotification(Article stageProduct, string transactionId, string userName, int userId, bool sendSeparateLive, string[] channels)
		{
			try
			{
				var stageProducts = new[] { stageProduct };
				var liveProducts = new[] { sendSeparateLive ? Productservice.GetProductById(stageProduct.Id, true) : stageProduct };
				DoWithLogging("NotificationService.SendProducts stage", transactionId, () => NotificationService.SendProducts(stageProducts, true, userName, userId, channels));
				DoWithLogging("NotificationService.SendProducts live", transactionId, () => NotificationService.SendProducts(liveProducts, false, userName, userId, channels));
			}
			catch (Exception ex)
			{
				throw new ProductException(stageProduct.Id, "Отправка на витрины", ex);
			}		
		}
		#endregion
	}
}