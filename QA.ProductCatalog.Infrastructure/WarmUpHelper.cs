using System;
using QA.Core.Logger;
using Unity;

namespace QA.ProductCatalog.Infrastructure
{
    public class WarmUpHelper
	{
		public static void WarmUp(IUnityContainer unityContainer)
		{
			var warmUpProviders = unityContainer.ResolveAll<IWarmUpProvider>();

			var logger = unityContainer.Resolve<ILogger>();

			foreach (var warmUpProvider in warmUpProviders)
				try
				{
					logger.Info(warmUpProvider.GetType().Name + ": warming up...");

					warmUpProvider.WarmUp();
				}
				catch (Exception ex)
				{
					logger.ErrorException("Error calling WarmUp on {0}", ex, warmUpProvider.GetType().Name);
				}

			logger.Info("Warming up complete");
		}
	}
}
