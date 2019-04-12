﻿using System;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.Infrastructure;
using Unity;

namespace QA.Core.DPC.Loader
{
    public class WarmUpHelper
	{
		public static void WarmUp()
		{
			var connectionProvider = ObjectFactoryBase.DefaultContainer.Resolve<IConnectionProvider>();
			if (!connectionProvider.QPMode)
			{
				var warmUpProviders = ObjectFactoryBase.DefaultContainer.ResolveAll<IWarmUpProvider>();				
				var logger = ObjectFactoryBase.Logger;
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
}
