using System;
using NLog;
using QA.Core.DPC.Loader.Services;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class RestoreAction : ArchiveProductActionBase
	{
		public RestoreAction(IArticleService articleService, IFieldService fieldService, IProductService productservice, Func<ITransaction> createTransaction, IQPNotificationService notificationService)
			: base(articleService, fieldService, productservice, createTransaction, notificationService)
		{
		}

		protected override bool NeedToArchive
		{
			get { return false; }
		}
	}
}
