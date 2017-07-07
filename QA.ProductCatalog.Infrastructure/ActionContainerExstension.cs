using Microsoft.Practices.Unity;
using System;
using QA.Core.DPC.QP.Services;

namespace QA.Core.ProductCatalog
{
	public static class ActionContainerExstension
	{
		public static void RegisterConnectionString(this IUnityContainer container, string connectionString)
		{
			container.RegisterInstance<IConnectionProvider>(new ExplicitConnectionProvider(connectionString));
		}

		public static string GetConnectionString(this IUnityContainer container)
		{
			return container.Resolve<IConnectionProvider>().GetConnection();
		}
	}
}