using System.Collections.Generic;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.DTO;
using System;
using System.Data;
using QA.Core.DPC.Loader.Services;
using NLog;

namespace QA.Core.ProductCatalog.Actions.Actions
{
	public class TestAction : ProductActionBase
	{
		public TestAction(IArticleService articleService, IFieldService fieldService, IProductService productservice, Func<ITransaction> createTransaction)
			: base(articleService, fieldService, productservice, createTransaction)
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