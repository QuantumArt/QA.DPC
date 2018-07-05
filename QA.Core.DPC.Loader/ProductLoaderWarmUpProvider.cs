using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader
{
    public class ProductLoaderWarmUpProvider : IWarmUpProvider
	{
		private readonly ProductLoader _productLoader;
		private readonly int _productIdToLoad;
		private readonly ILogger _logger;

		public ProductLoaderWarmUpProvider(ProductLoader productLoader, ILogger logger,int productIdToLoad)
		{
			_productLoader = productLoader;

			_productIdToLoad = productIdToLoad;

			_logger = logger;
		}

		public void WarmUp()
		{
			_productLoader.GetProductById(_productIdToLoad);
		}
	}
}
