using System;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.DPC.Loader
{
    public class ProductLoaderWarmUpProvider : IWarmUpProvider
	{
		private readonly ProductLoader _productLoader;
		private readonly LoaderProperties _props;
		private readonly ILogger _logger;
		

		public ProductLoaderWarmUpProvider(ProductLoader productLoader, ILogger logger, IOptions<LoaderProperties> props)
		{
			_productLoader = productLoader;
			_props = props.Value;
			_logger = logger;
		}

		public void WarmUp()
		{
			if (_props.LoaderWarmUpProductId != 0)
			{
				var warmMessage = $"Warming up with product {_props.LoaderWarmUpProductId}";
				_logger.Info($"{warmMessage} started.");

				var sw = new Stopwatch();
				bool warmUpErrorOccurs = false;
				sw.Start();
				try
				{
					_productLoader.GetProductById(_props.LoaderWarmUpProductId);
				}
				catch (Exception ex)
				{
					warmUpErrorOccurs = true;
					_logger.ErrorException($"{warmMessage} failed", ex);
				}
				
				sw.Stop();

				if (!warmUpErrorOccurs)
				{
					_logger.Info($"{warmMessage} finished. Took {sw.Elapsed.TotalSeconds} sec.");			
				}
			}

		}
	}
}
