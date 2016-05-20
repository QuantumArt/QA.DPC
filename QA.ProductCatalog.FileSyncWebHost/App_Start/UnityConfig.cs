using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Core;

namespace QA.ProductCatalog.FileSyncWebHost
{
	public static class UnityConfig
	{
		public static IUnityContainer Configure()
		{
			return ObjectFactoryConfigurator.InitializeWith(RegisterTypes(new UnityContainer()));
		}

		private static IUnityContainer RegisterTypes(IUnityContainer unityContainer)
		{

			unityContainer.LoadConfiguration("Default");

			return unityContainer;
		}
	}
}