using System.Collections.Generic;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using QA.Core.DPC.Loader.Services;
using QA.Core.Logger;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class TestAction : ActionBase
	{
		public TestAction(IArticleService articleService, IFieldService fieldService, IProductService productservice, ILogger logger, Func<ITransaction> createTransaction)
			: base(articleService, fieldService, productservice, logger, createTransaction)
		{
		}

		protected override void ProcessProduct(int productId, Dictionary<string, string> actionParameters)
		{
			if (productId == 0)
			{
				var result = MessageResult.Error("MessageResult Error", new[]{ 1, 2, 3});
				ValidateMessageResult(productId, result);
			}
			if (productId % 2 == 0)
			{
				throw new Exception();
			}
		}        
	}
}