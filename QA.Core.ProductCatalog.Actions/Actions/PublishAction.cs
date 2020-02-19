using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using QA.Core.DPC.Loader.Services;
using QA.Core.DPC.Resources;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

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

	        string[] channels = actionParameters.GetChannels();
            bool localize = actionParameters.GetLocalize();

            string ignoredStatus = (actionParameters.ContainsKey("IgnoredStatus")) ? actionParameters["IgnoredStatus"] : null;
			var ignoredStatuses = ignoredStatus?.Split(',') ?? Enumerable.Empty<string>().ToArray();

            var product = DoWithLogging(
	            () => Productservice.GetProductById(productId), 
	            "Getting product {id}", productId
	        );
            
            if (product == null)
	            throw new ProductException(productId, nameof(TaskStrings.ProductsNotFound));
            
            ProductIds.Add(product.Id);
            if (ignoredStatuses.Contains(product.Status))
	            throw new ProductException(product.Id, nameof(TaskStrings.ProductsExcludedByStatus));

			if (!ArticleFilter.DefaultFilter.Matches(product))
				throw new ProductException(product.Id, nameof(TaskStrings.ProductsNotToPublish));

			var state = DoWithLogging(
				() => FreezeService.GetFreezeState(productId), 
				"Getting freezing state for product {id}", productId
			);

            if (state == FreezeState.Frozen)
            {
	            throw new ProductException(product.Id, nameof(TaskStrings.ProductsFreezed));
            }

            var xamlValidationErrors = DoWithLogging(
	            () => ArticleService.XamlValidationById(product.Id, true), 
	            "Validating XAML for product {id}", productId
	        );
            
            var validationResult = ActionTaskResult.FromRulesException(xamlValidationErrors, product.Id);

            if (!validationResult.IsSuccess)
            {
                ValidationErrors.TryAdd(product.Id, validationResult.ToString());
                throw new ProductException(product.Id, JsonConvert.SerializeObject(validationResult));
            }

            var allArticles = DoWithLogging(
	            () => GetAllArticles(new[] { product }).ToArray(),
	            "Getting all articles for product {id}", productId
	         );
            
			bool containsIgnored = allArticles.Any(a => ignoredStatuses.Contains(a.Status));
			
			var articleIds = allArticles
				.Where(a => a.Id != productId && !a.IsPublished && !ignoredStatuses.Contains(a.Status))
				.Select(a => a.Id)
				.Distinct()
				.ToArray();

            var result = DoWithLogging(
	            () => ArticleService.Publish(product.ContentId, new[] { productId }),
	            "Publishing product {id}", productId
	        );
            
            ValidateMessageResult(productId, result);

            if (articleIds.Any())
            {
	            DoWithLogging(
		            () => ArticleService.SimplePublish(articleIds),
		            "Publishing articles {ids} for product {id}", articleIds, productId
		        );	            
            }

            if (state == FreezeState.Unfrosen)
            {
	            DoWithLogging(
		            () => FreezeService.ResetFreezing(product.Id),
		            "Reset freezing for product {id}", productId
		        );	  
               
            }

            const string doNotSendNotificationsKey = "DoNotSendNotifications";
			bool doNotSendNotifications = actionParameters.ContainsKey(doNotSendNotificationsKey) && bool.Parse(actionParameters[doNotSendNotificationsKey]);

	        if (!doNotSendNotifications)
	        {
		        SendNotification(product, UserName, UserId, containsIgnored, localize, channels);
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

		private void SendNotification(Article stageProduct, string userName, int userId, bool sendSeparateLive, bool localize, string[] channels)
		{
			try
			{
				var stageProducts = new[] { stageProduct };
				var liveProducts = new[] { stageProduct };
				var productId = stageProduct.Id;
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
	                if (sendSeparateLive)
	                {
		                liveProducts = DoWithLogging(
			                () => new[] {Productservice.GetProductById(stageProduct.Id, true)},
			                "Receiving separate live product {id}", productId
			            );
	                }
	                
                    DoWithLogging(
	                    () => NotificationService.SendProducts(stageProducts, true, userName, userId, localize, false, channels),
	                    "Sending stage notifications for product {id}", productId
	                );
                    
                    DoWithLogging(
	                    () => NotificationService.SendProducts(liveProducts, false, userName, userId, localize, false, channels),
	                    "Sending live notifications for product {id}", productId
	                );
                }
			}
			catch (Exception ex)
			{
				throw new ProductException(stageProduct.Id, nameof(TaskStrings.NotificationSenderError), ex);
			}		
		}
		#endregion
	}
}