using System;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation.Validators;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.ProductCatalog.Validation.Configuration
{
    public static class ConfigurationExstensions
	{
		public static IUnityContainer RegisterRemoteValidators<T>(this IUnityContainer container)
			where T : UnityContainerExtension
		{
			var a = typeof(T).Assembly;
			foreach (var t in a.GetExportedTypes())
			{
			    if (typeof(IRemoteValidator2).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
			    {
			        container.RegisterType(typeof(IRemoteValidator2), t, t.Name);
			    }
            }
			return container;
		}


		public static IUnityContainer RegisterRemoteValidator(this IUnityContainer container, string key, Func<IUnityContainer, IRemoteValidator2[]> factory)		
		{
			return container.RegisterFactory<IRemoteValidator2>(key, c => new RemoteValidatorDecorator(factory(c)));
		}
	}
}
