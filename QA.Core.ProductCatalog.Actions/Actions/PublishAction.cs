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
using System.Transactions;
using NLog;
using QA.ProductCatalog.ContentProviders;
using System.Collections.Concurrent;
using System.Data;
using QA.ProductCatalog.Integration;

namespace QA.Core.ProductCatalog.Actions
{
    public class PublishAction : ProductActionBase
    {
        protected IQPNotificationService NotificationService { get; }
        protected IXmlProductService XmlProductService { get; }
        protected IFreezeService FreezeService { get; }
        protected IValidationService ValidationService { get; }
        protected ConcurrentDictionary<int, string> ValidationErrors { get; private set; }
        protected List<int> ProductIds { get; private set; }

        public PublishAction(
            IArticleService articleService,
            IFieldService fieldService,
            IProductService productservice,
            Func<ITransaction> createTransaction,
            IQPNotificationService notificationService,
            IXmlProductService xmlProductService,
            IFreezeService freezeService,
            IValidationService validationService
            )
            : base(articleService, fieldService, productservice, createTransaction)
        {
            NotificationService = notificationService;
            XmlProductService = xmlProductService;
            FreezeService = freezeService;
            ValidationService = validationService;
            ValidationErrors = new ConcurrentDictionary<int, string>();
            ProductIds = new List<int>();
        }

        #region Overrides
        protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
        {
            var transactionId = Guid.NewGuid().ToString();

			string[] channels = actionParameters.GetChannels();
            bool localize = actionParameters.GetLocalize();

            string ignoredStatus = (actionParameters.ContainsKey("IgnoredStatus")) ? actionParameters["IgnoredStatus"] : null;
			var ignoredStatuses = ignoredStatus?.Split(',') ?? Enumerable.Empty<string>().ToArray();

            var product = DoWithLogging("Productservice.GetProductById", transactionId, () => Productservice.GetProductById(productId));
            ProductIds.Add(product.Id);

			if (ignoredStatuses.Contains(product.Status))
				ValidateMessageResult(product.Id, MessageResult.Error("ProductsExcludedByStatus"));

			if (!ArticleFilter.DefaultFilter.Matches(product))
				ValidateMessageResult(product.Id, MessageResult.Error("ProductsNotToPublish"));

            var state = FreezeService.GetFreezeState(product.Id);

            if (state == FreezeState.Frozen)
            {
                ValidateMessageResult(product.Id, MessageResult.Error("ProductsFreezed"));
            }

            var xamlValidationErrors = DoWithLogging("ValidateXaml", transactionId, () => ArticleService.XamlValidationById(product.Id, true));

            if (!xamlValidationErrors.IsEmpty)
            {
                ValidationErrors.TryAdd(product.Id, string.Join(Environment.NewLine, xamlValidationErrors.Errors.Select(s => s.Message)));
                ValidateMessageResult(product.Id, MessageResult.Error(string.Join(@";" + Environment.NewLine, xamlValidationErrors.Errors.Select(s => s.Message))));
            }

            var allArticles = GetAllArticles(new[] { product }).ToArray();
			bool containsIgnored = allArticles.Any(a => ignoredStatuses.Contains(a.Status));

            var articleIds = DoWithLogging("GetAllArticles", transactionId, () => allArticles.Where(a => a.Id != productId && !a.IsPublished && !ignoredStatuses.Contains(a.Status)).Select(a => a.Id).Distinct().ToArray());

            var result = DoWithLogging("ArticleService.Publish", transactionId, () => ArticleService.Publish(product.ContentId, new[] { productId }));
            ValidateMessageResult(productId, result);

            DoWithLogging("ArticleService.SimplePublish", transactionId, () => ArticleService.SimplePublish(articleIds));

            if (state == FreezeState.Unfrosen)
            {
	            HttpContextUserProvider.ForcedUserId = UserId;
                FreezeService.ResetFreezing(new []{ product.Id} );
                HttpContextUserProvider.ForcedUserId = 0;
            }

            const string doNotSendNotificationsKey = "DoNotSendNotifications";
			bool doNotSendNotifications = actionParameters.ContainsKey(doNotSendNotificationsKey) && bool.Parse(actionParameters[doNotSendNotificationsKey]);

	        if (!doNotSendNotifications)
			{
				DoWithLogging("SendNotification (includes substeps)", transactionId, () => SendNotification(product, transactionId, UserName, UserId, containsIgnored, localize, channels));
            }          
        }

        protected override void OnStartProcess()
        {
            ProductIds.Clear();
            ValidationErrors.Clear();
        }

        protected override void OnEndProcess()
        {
            using (var transaction = CreateTransaction())
            {
                ValidationService.UpdateValidationInfo(ProductIds.ToArray(), ValidationErrors);
                transaction.Commit();
            }
        }
        #endregion

        /// <exception cref="MessageResultException"/>
        public void PublishProduct(int productId, Dictionary<string, string> actionParameters)
        {
            var userProvider = ObjectFactoryBase.Resolve<IUserProvider>();

            UserId = userProvider.GetUserId();
            UserName = userProvider.GetUserName();

            using (var transaction = CreateTransaction())
            {
                ProcessProduct(productId, actionParameters ?? new Dictionary<string, string>());
                transaction.Commit();
            }
        }

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

		private void SendNotification(Article stageProduct, string transactionId, string userName, int userId, bool sendSeparateLive, bool localize, string[] channels)
		{
			try
			{
				var stageProducts = new[] { stageProduct };
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
	                var liveProducts = new[] { sendSeparateLive ? Productservice.GetProductById(stageProduct.Id, true) : stageProduct };	                
                    DoWithLogging("NotificationService.SendProducts stage", transactionId, () => NotificationService.SendProducts(stageProducts, true, userName, userId, localize, false, channels));
                    DoWithLogging("NotificationService.SendProducts live", transactionId, () => NotificationService.SendProducts(liveProducts, false, userName, userId, localize, false, channels));
                }
			}
			catch (Exception ex)
			{
				throw new ProductException(stageProduct.Id, "Sending to fronts", ex);
			}		
		}
		#endregion
	}
}