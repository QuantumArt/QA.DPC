using System.Collections.Generic;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;
using System;
using QA.Core.DPC.Loader.Services;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	internal class ActionBaseFake : ActionBase
	{
		public int LastProductId { get; set; }
		public Exception ExceptionToThrow { get; set; }

		public ActionBaseFake(IArticleService articleService, IFieldService fieldService, IProductService productservice, ILogger logger, Func<ITransaction> createTransaction)
			: base(articleService, fieldService, productservice, logger, createTransaction)
		{
		}

		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
			LastProductId = productId;

			if (ExceptionToThrow != null)
			{
				throw ExceptionToThrow;
			}
		}
	}
}
