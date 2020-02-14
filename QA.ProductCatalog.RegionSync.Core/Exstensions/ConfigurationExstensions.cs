using QA.ProductCatalog.RegionSync.Core.Services;
using Unity;
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
				.RegisterFactory<IRegionProvider<TModel>>(
					c => new RegionProvider<TModel, IRegionProviderConfiguration>(
						c.Resolve<IRegionProviderConfiguration>(key)
					)
				);
		}

		public static IUnityContainer RegisterRegionProviderConfiguration(this IUnityContainer container, string key)
		{
			return container.RegisterFactory<IRegionProviderConfiguration>(
				c => new RegionProviderConfiguration(key), 
				new HierarchicalLifetimeManager()
			);
		}	
	}
}