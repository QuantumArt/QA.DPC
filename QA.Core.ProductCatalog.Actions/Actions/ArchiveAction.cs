﻿using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using QA.Core.DPC.Loader.Services;
using NLog;
using QA.Core.DPC.Resources;
using QA.Core.Models.Entities;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class ArchiveAction : ArchiveProductActionBase
	{
		public ArchiveAction(IArticleService articleService, IFieldService fieldService, IProductService productservice, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productservice, createTransaction, notificationService)
		{
		}

		protected override bool NeedToArchive
		{
			get { return true; }
		}


		protected override Article[] GetNotificationProducts(int productId)
		{
			try
			{
				return Productservice.GetSimpleProductsByIds(new[] { productId });
			}
			catch (Exception ex)
			{
				throw new ProductException(productId, "Product notification preparing failed", ex) { IsError = true };
			}
		}

		protected override void SendNotification(Article[] products, int productId, string[] channels)
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
				throw new ProductException(productId, nameof(TaskStrings.NotificationSenderError), ex) { IsError = true };
			}
		}


	}
}
