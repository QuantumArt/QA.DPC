using System;
using System.Diagnostics;
using QA.Core.Cache;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader
{
    public class ProductLoaderWarmUpProvider : IWarmUpProvider
	{
		private readonly ProductLoader _productLoader;
		private readonly int _productIdToLoad;
		private readonly ILogger _logger;
		

		public ProductLoaderWarmUpProvider(ProductLoader productLoader, ILogger logger, int productIdToLoad)
		{
			_productLoader = productLoader;
			_productIdToLoad = productIdToLoad;
			_logger = logger;
		}

		public void WarmUp()
		{
			_logger.Info($"Warming up with product {_productIdToLoad} started.");

			var sw = new Stopwatch();
			sw.Start();
			_productLoader.GetProductById(_productIdToLoad);
			sw.Stop();
			
			_logger.Info($"Warming up with product {_productIdToLoad} finished. Took {sw.Elapsed.TotalSeconds} sec.");
		}
	}
}
