using System;
using Microsoft.Practices.Unity;
using QA.Core;
using QA.Core.Logger;
using QA.ProductCatalog.RegionSync.Core.Services;

namespace QA.ProductCatalog.RegionSync.Core.Exstensions
{
	public static class ConfigurationExstensions
	{
		public static IUnityContainer RegisterRegionProvider<TModel>(this IUnityContainer container, string key)
			where TModel : class
		{
			return container
				.RegisterRegionProviderConfiguration(key)
				.RegisterType<IRegionProvider<TModel>, RegionProvider<TModel, IRegionProviderConfiguration>>(
					key,
					new InjectionFactory(c => new RegionProvider<TModel, IRegionProviderConfiguration>(c.Resolve<IRegionProviderConfiguration>(key)))
				);
		}

		public static IUnityContainer RegisterRegionProviderConfiguration(this IUnityContainer container, string key)
		{
			return container.RegisterType<IRegionProviderConfiguration, RegionProviderConfiguration>(
				key,
				new HierarchicalLifetimeManager(),
				new InjectionFactory(c => new RegionProviderConfiguration(
					key,
					c.Resolve<ILogger>()
				)
			));
		}	
	}
}