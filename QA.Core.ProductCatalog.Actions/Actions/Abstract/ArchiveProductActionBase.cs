﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QA.Core.DPC.Loader.Services;
using QA.Core.Models.Configuration;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL;
using System.Transactions;
using QA.Core.DPC.Resources;
using NLog;
using QA.Core.ProductCatalog.Actions.Exceptions;
using Quantumart.QP8.BLL.Services.DTO;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public abstract class ArchiveProductActionBase : ProductActionBase
	{
		private const string DoNotSendNotificationsKey = "DoNotSendNotifications";
		protected IQPNotificationService NotificationService { get; private set; }

		protected ArchiveProductActionBase(IArticleService articleService, IFieldService fieldService, IProductService productService, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productService, createTransaction)
		{
			NotificationService = notificationService;
		}

		#region Abstract and virtual methods
		protected abstract bool NeedToArchive { get; }

		protected virtual QA.Core.Models.Entities.Article[] GetNotificationProducts(int productId)
		{
			return null;
		}
		protected virtual void SendNotification(QA.Core.Models.Entities.Article[] products, int productId, string[] channels)
		{
		}
		#endregion

		#region Overrides
		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
            Dictionary<int, Product<DeletingMode>> dictionary;
            QA.Core.Models.Entities.Article[] notificationProducts = null;
            bool doNotSendNotifications = actionParameters.ContainsKey(DoNotSendNotificationsKey) && bool.Parse(actionParameters[DoNotSendNotificationsKey]);
            bool excludeArchive = NeedToArchive;
		    var product = ArticleService.Read(productId, excludeArchive);
		    if (product == null)
		    {
			    throw new ProductException(productId, nameof(TaskStrings.ProductsNotFound));
		    }
			var definition = Productservice.GetProductDefinition(0, product.ContentId);
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
            { 
	            dictionary = DoWithLogging(
		            () => GetProductsToBeProcessed(product, definition, ef => ef.DeletingMode, DeletingMode.Delete, excludeArchive),
		            "Getting products to be processed for product {id}", productId
	            );
			
			    if (!doNotSendNotifications)
                    notificationProducts = GetNotificationProducts(productId);
            }

            var articleIds = dictionary.Values
	            .Where(p => p.Article.Id != product.Id && p.Article.Archived != NeedToArchive)
	            .Select(p => p.Article.Id)
	        .ToArray();

            var result = DoWithLogging(
	            () => ArticleService.SetArchiveFlag(product.ContentId, new[] {product.Id}, NeedToArchive),
				"Set archive flag in {flag} for product {id}", NeedToArchive, product.Id 
            );
            
            ValidateMessageResult(product.Id, result);

            if (articleIds.Any())
            {
	            DoWithLogging(
		            () => ArticleService.SimpleSetArchiveFlag(articleIds, NeedToArchive),
		            "Set archive flag in {flag} for articles {ids}", NeedToArchive, articleIds
	            );           
            }

            if (!doNotSendNotifications)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    string[] channels = actionParameters.GetChannels();
                    SendNotification(notificationProducts, productId, channels);
                }
            }
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