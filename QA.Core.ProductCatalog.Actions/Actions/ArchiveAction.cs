using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using QA.Core.DPC.Loader.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class ArchiveAction : ArchiveActionBase
	{
		public ArchiveAction(IArticleService articleService, IFieldService fieldService, IProductService productservice, ILogger logger, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productservice, logger, createTransaction, notificationService)
		{
		}

		protected override bool NeedToArchive
		{
			get { return true; }
		}


		protected override Article[] PrepareNotification(int productId)
		{
			try
			{
				return Productservice.GetSimpleProductsByIds(new[] { productId });
			}
			catch (Exception ex)
			{
				throw new ProductException(productId, "не удалось подготовить уведомление об архивации", ex);
			}
		}

		protected override void SendNotification(Article[] products, int productId, string[] channels)
		{
			try
			{
				NotificationService.DeleteProducts(products, UserName, UserId, false, channels);
			}
			catch (Exception ex)
			{
				throw new ProductException(productId, "не удалось отправить уведомление об архивации", ex);
			}
		}


	}
}
