using System;
using System.ServiceProcess;
using Microsoft.Practices.Unity;
using QA.Scheduler.API.Models;
using QA.Scheduler.Core.Service;

namespace QA.Scheduler.Core.Configuration
{
	internal class ServiceCoreConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			var descriptors = Container.ResolveAll<ServiceDescriptor>();

			foreach (var descriptor in descriptors)
			{
				Container.RegisterType<ServiceBase, SchedulerService>(descriptor.Name, new InjectionFactory(c => new SchedulerService(c.Resolve<Func<IUnityContainer>>(descriptor.Key), descriptor)));
			}
		}
	}
}
