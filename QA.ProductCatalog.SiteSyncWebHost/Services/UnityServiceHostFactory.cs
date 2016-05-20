using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace QA.ProductCatalog.SiteSyncWebHost.Services
{
	public class UnityServiceHostFactory : ServiceHostFactory
	{
		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			var serviceHost = new UnityServiceHost(serviceType, baseAddresses);
			serviceHost.Container = UnityConfig.Configure();
			return serviceHost;
		}
	}
}