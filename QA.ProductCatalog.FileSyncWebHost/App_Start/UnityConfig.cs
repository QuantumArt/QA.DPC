using Microsoft.Practices.Unity.Configuration;
using QA.Core;
using Unity;

namespace QA.ProductCatalog.FileSyncWebHost
{
    public static class UnityConfig
	{
		public static IUnityContainer Configure()
		{
		    var container = RegisterTypes(new UnityContainer());
		    ObjectFactoryConfigurator.DefaultContainer = container;
		    return container;
        }

		private static IUnityContainer RegisterTypes(IUnityContainer unityContainer)
		{

			unityContainer.LoadConfiguration("Default");

			return unityContainer;
		}
	}
}