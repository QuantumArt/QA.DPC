using System;
using System.Collections.Generic;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using System.Transactions;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public abstract class ArchiveActionBase : ActionBase
	{
		private const string DoNotSendNotificationsKey = "DoNotSendNotifications";
		protected IQPNotificationService NotificationService { get; private set; }

		protected ArchiveActionBase(IArticleService articleService, IFieldService fieldService, IProductService productService, ILogger logger, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productService, logger, createTransaction)
		{
			NotificationService = notificationService;
		}

		#region Abstract and virtual methods
		protected abstract bool NeedToArchive { get; }

		protected virtual QA.Core.Models.Entities.Article[] PrepareNotification(int productId)
		{
			return null;
		}
		protected virtual void SendNotification(QA.Core.Models.Entities.Article[] products, int productId)
		{
		}
		#endregion

		#region Overrides
		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
			var product = ArticleService.Read(productId);
			var definition = Productservice.GetProductDefinition(0, product.ContentId);
			var dictionary = GetProductsToBeProcessed<DeletingMode>(product, definition, ef => ef.DeletingMode, DeletingMode.Delete);
			bool doNotSendNotifications = actionParameters.ContainsKey(DoNotSendNotificationsKey) && bool.Parse(actionParameters[DoNotSendNotificationsKey]);
			QA.Core.Models.Entities.Article[] notificationProducts = null;
            if (!doNotSendNotifications)
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    notificationProducts = PrepareNotification(productId);
                }

			ArchiveProducts(dictionary, product);

			
			if (!doNotSendNotifications)
				SendNotification(notificationProducts, productId);
		}
		#endregion

		#region Private methods
		private void ArchiveProducts(Dictionary<int, Product<DeletingMode>> dictionary, Article product)
		{
			var articleIds = dictionary.Values.Where(p => p.Article.Id != product.Id && p.Article.Archived != NeedToArchive).Select(p => p.Article.Id).ToArray();

			var result = ArticleService.SetArchiveFlag(product.ContentId, new[] {product.Id}, NeedToArchive);
			ValidateMessageResult(product.Id, result);
			ArticleService.SimpleSetArchiveFlag(articleIds, NeedToArchive);
		}
		#endregion
	}
}