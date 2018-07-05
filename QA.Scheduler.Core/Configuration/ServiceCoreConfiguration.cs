using System;
using System.ServiceProcess;
using QA.Scheduler.API.Models;
using QA.Scheduler.Core.Service;
using Unity;
using Unity.Extension;
using Unity.Injection;

namespace QA.Scheduler.Core.Configuration
{
    internal class ServiceCoreConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			var descriptors = Container.ResolveAll<ServiceDescriptor>();

			foreach (var descriptor in descriptors)
			{
				Container.RegisterType<ServiceBase>(descriptor.Name, new InjectionFactory(c => new SchedulerService(c.Resolve<Func<IUnityContainer>>(descriptor.Key), descriptor)));
			}
		}
	}
}
