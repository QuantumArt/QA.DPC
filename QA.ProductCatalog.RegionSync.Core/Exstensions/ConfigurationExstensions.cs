using QA.Core.Logger;
using QA.ProductCatalog.RegionSync.Core.Services;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.ProductCatalog.RegionSync.Core.Exstensions
{
    public static class ConfigurationExstensions
	{
		public static IUnityContainer RegisterRegionProvider<TModel>(this IUnityContainer container, string key)
			where TModel : class
		{
			return container
				.RegisterRegionProviderConfiguration(key)
				.RegisterType<IRegionProvider<TModel>>(
					key,
					new InjectionFactory(c => new RegionProvider<TModel, IRegionProviderConfiguration>(c.Resolve<IRegionProviderConfiguration>(key)))
				);
		}

		public static IUnityContainer RegisterRegionProviderConfiguration(this IUnityContainer container, string key)
		{
			return container.RegisterType<IRegionProviderConfiguration>(
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