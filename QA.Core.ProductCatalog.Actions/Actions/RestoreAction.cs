using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class RestoreAction : ArchiveActionBase
	{
		public RestoreAction(IArticleService articleService, IFieldService fieldService, IProductService productservice, ILogger logger, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productservice, logger, createTransaction, notificationService)
		{
		}

		protected override bool NeedToArchive
		{
			get { return false; }
		}
	}
}
