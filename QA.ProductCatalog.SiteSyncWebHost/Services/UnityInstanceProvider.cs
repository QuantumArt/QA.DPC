using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Unity;

namespace QA.ProductCatalog.SiteSyncWebHost.Services
{
    public class UnityInstanceProvider : IInstanceProvider
	{
		public IUnityContainer Container { set; get; }
		public Type ServiceType { set; get; }

		public UnityInstanceProvider()
			: this(null)
		{
		}

		public UnityInstanceProvider(Type type)
		{
			ServiceType = type;
			Container = new UnityContainer();
		}

		#region IInstanceProvider implementation

		public object GetInstance(InstanceContext instanceContext, Message message)
		{
			return Container.Resolve(ServiceType);
		}

		public object GetInstance(InstanceContext instanceContext)
		{
			return GetInstance(instanceContext, null);
		}
		public void ReleaseInstance(InstanceContext instanceContext, object instance)
		{
		}
		#endregion
	}
}