using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using Unity;

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
		
		public static Customer GetCustomer(this IUnityContainer container)
		{
			return container.Resolve<IConnectionProvider>().GetCustomer();
		}
		
	}
}