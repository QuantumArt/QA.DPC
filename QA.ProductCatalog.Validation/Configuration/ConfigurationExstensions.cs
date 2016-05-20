using System;
using System.Linq;
using Microsoft.Practices.Unity;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Validation.Validators;

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
				if (typeof(IRemoteValidator).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
				{
					container.RegisterType(typeof(IRemoteValidator), t, t.Name);
				}
			}
			return container;
		}

		public static IUnityContainer RegisterRemoteValidator(this IUnityContainer container, string key, string[] keys)
		{
			return container.RegisterType<IRemoteValidator, RemoteValidatorDecorator>(key, new InjectionFactory(c => new RemoteValidatorDecorator(keys.Select(k => c.Resolve<IRemoteValidator>(k)).ToArray())));
		}

		public static IUnityContainer RegisterRemoteValidator(this IUnityContainer container, string key, Func<IUnityContainer, IRemoteValidator[]> factory)		
		{
			return container.RegisterType<IRemoteValidator, RemoteValidatorDecorator>(key, new InjectionFactory(c => new RemoteValidatorDecorator(factory(c))));
		}

		public static IUnityContainer RegisterRemoteValidator<T1, T2>(this IUnityContainer container, string key)
			where T1 : IRemoteValidator
			where T2 : IRemoteValidator
		{
			return container.RegisterRemoteValidator(key, c => new IRemoteValidator[] { c.Resolve<T1>(), c.Resolve<T2>() });
		}

		public static IUnityContainer RegisterRemoteValidator<T1, T2, T3>(this IUnityContainer container, string key)
			where T1 : IRemoteValidator
			where T2 : IRemoteValidator
			where T3 : IRemoteValidator
		{
			return container.RegisterRemoteValidator(key, c => new IRemoteValidator[] { c.Resolve<T1>(), c.Resolve<T2>(), c.Resolve<T3>() });
		}

	}
}
