using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using System.Transactions;
using NLog;
using QA.Core.DPC.Resources;
using QA.Core.Models.Entities;

namespace QA.Core.ProductCatalog.Actions
{
	public class DeleteAction : ProductActionBase
	{
		private const string DoNotSendNotificationsKey = "DoNotSendNotifications";
		protected IQPNotificationService NotificationService { get; private set; }

		public DeleteAction(IArticleService articleService, IFieldService fieldService, IProductService productService, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productService, createTransaction)
		{
			NotificationService = notificationService;
		}

		#region Overrides
		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
            string[] channels = actionParameters.GetChannels();
            var product = ArticleService.Read(productId, false);
            
            if (product == null)
	            throw new ProductException(productId, nameof(TaskStrings.ProductsNotFound));
            
            var definition = Productservice.GetProductDefinition(0, product.ContentId);
			bool doNotSendNotifications = actionParameters.ContainsKey(DoNotSendNotificationsKey) && bool.Parse(actionParameters[DoNotSendNotificationsKey]);

            DeleteProduct(product, definition, doNotSendNotifications, true, channels);
		}
        #endregion

        public void DeleteProduct(Quantumart.QP8.BLL.Article product, ProductDefinition definition)
        {
            using (var transaction = CreateTransaction())
            {
                DeleteProduct(product, definition, true, false, null);
                transaction.Commit();
            }
        }
        
        public void DeleteProduct(Quantumart.QP8.BLL.Article product, ProductDefinition definition, bool doNotSendNotifications, bool checkRootArticlePermissions, string[] channels)
	    {
            Dictionary<int, Product<DeletingMode>> dictionary;
            Article[] products;            

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
	            dictionary = DoWithLogging(
		            () => GetProductsToBeProcessed(product, definition, ef => ef.DeletingMode, DeletingMode.Delete, false),
		            "Receiving articles to be deleted for product {id}", product.Id
	            );
                products = doNotSendNotifications ? null : Productservice.GetSimpleProductsByIds(new[] { product.Id });
            }

            DeleteProducts(dictionary, product.Id, checkRootArticlePermissions);

            if (!doNotSendNotifications)
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    SendNotification(products, product.Id, channels);
                }
		}

		#region Private methods
		private void DeleteProducts(Dictionary<int, Product<DeletingMode>> dictionary, int productId, bool checkRootArticlePermissions)
		{
			var articleIds = dictionary.Values.Where(a => a.Article.Id != productId).Select(p => p.Article.Id).ToArray();
			DoWithLogging(
				() => DeleteRootArticle(productId, checkRootArticlePermissions),
				"Deleting root article for product {id}", productId
			);

		    DoWithLogging(
			    () => ArticleService.SimpleDelete(articleIds),
			    "Deleting articles {ids} for product {id}", articleIds, productId
		    );
		}
		
		private void DeleteRootArticle(int productId, bool checkRootArticlePermissions)
		{
			if (checkRootArticlePermissions)
			{
				var result = ArticleService.Delete(productId);
				ValidateMessageResult(productId, result);
			}
			else
				ArticleService.SimpleDelete(new[] {productId});
		}

		private void SendNotification(Models.Entities.Article[] products, int productId, string[] channels)
		{
			try
			{
				DoWithLogging(
					() => NotificationService.DeleteProducts(products, UserName, UserId, false, channels),
					"Sending delete notifications for product {id}", productId
				);
			}
			catch (Exception ex)
			{
				throw new ProductException(productId, nameof(TaskStrings.NotificationSenderError), ex);
			}
		}
		#endregion
	}
}