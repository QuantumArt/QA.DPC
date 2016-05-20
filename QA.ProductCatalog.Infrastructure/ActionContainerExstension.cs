using Microsoft.Practices.Unity;
using System;

namespace QA.Core.ProductCatalog
{
	public static class ActionContainerExstension
	{
		private const string ConnectionStringKey = "сonnectionString";

		public static void RegisterConnectionString(this IUnityContainer container, Func<IUnityContainer, string> factory)
		{
			container.RegisterType<string>(ConnectionStringKey, new InjectionFactory(factory));
		}

		public static void RegisterConnectionString(this IUnityContainer container, string connectionString)
		{
			container.RegisterConnectionString(c => connectionString);
		}

		public static string GetConnectionString(this IUnityContainer container)
		{
			return container.Resolve<string>(ConnectionStringKey);
		}
	}
}