using System;
using QA.Core.Cache;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader
{
    public class ProductLoaderWarmUpProvider : IWarmUpProvider
	{
		private readonly ProductLoader _productLoader;
		private readonly IVersionedCacheProvider _cacheProvider;
		private readonly int _productIdToLoad;
		private readonly ILogger _logger;
		

		public ProductLoaderWarmUpProvider(ProductLoader productLoader, ILogger logger, IVersionedCacheProvider cacheProvider, int productIdToLoad)
		{
			_productLoader = productLoader;
			_productIdToLoad = productIdToLoad;
			_logger = logger;
			_cacheProvider = cacheProvider;
		}

		public void WarmUp()
		{
			_cacheProvider.GetOrAdd("ProductLoaderWarmUpProvider_WarmUpData", TimeSpan.FromMinutes(10), Load);
		}

		private Article Load()
		{
			return _productLoader.GetProductById(_productIdToLoad);
		}
	}
}
