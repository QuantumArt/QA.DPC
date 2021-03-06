﻿using System.Collections.Generic;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Services;
using QA.ProductCatalog.Infrastructure;
using System;
using QA.Core.DPC.Loader.Services;
using QA.Core.Logger;

namespace QA.Core.ProductCatalog.Actions.Tests.Fakes
{
	internal class ActionBaseFake : ProductActionBase
	{
		public int LastProductId { get; set; }
		public Exception ExceptionToThrow { get; set; }

		public ActionBaseFake(IArticleService articleService, IFieldService fieldService, IProductService productservice, Func<ITransaction> createTransaction)
			: base(articleService, fieldService, productservice, createTransaction)
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
