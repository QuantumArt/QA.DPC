using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity.Configuration;
using QA.Scheduler.API.Services;
using QA.Scheduler.API.Models;
using QA.Scheduler.Core.Schedules;
using QA.Core.Logger;
using Unity;
using Unity.Extension;
using Unity.Injection;
using Unity.Lifetime;

namespace QA.Scheduler.Core.Configuration
{
    internal class SchedulerCoreConfiguration : UnityContainerExtension
	{
		protected override void Initialize()
		{
			Container.RegisterType<ISchedule, NullSchedule>(ConfigurationExstension.DefaultScheduleName);

			var descriptors = Container.ResolveAll<ProcessorDescriptor>();
			var services = descriptors.Select(d => d.Service).Distinct();


			foreach (var service in services)
			{
				Container.RegisterFactory<Func<IUnityContainer>>(service, parent =>
				{
					Func<IUnityContainer> factory = () =>
					{
						var container = parent.CreateChildContainer();
					
						try
						{
							container.LoadConfiguration();
							container.LoadConfiguration(service);
						}
						catch (Exception)
						{
						}

						container.RegisterFactory<ILogger>(c => c.Resolve<ILogger>(service));
						container.RegisterFactory<ServiceDescriptor>(c => c.Resolve<ServiceDescriptor>(service));
						container.RegisterFactory<IScheduler>(c => new Scheduler(c.Resolve<IEnumerable<IProcessor>>(), c.Resolve<ILogger>()), new HierarchicalLifetimeManager());
						Container.RegisterFactory<IEnumerable<IProcessor>>(c =>
							from d in descriptors
							where d.Service == service
							select new ScheduledProcessor(c.Resolve<Func<IProcessor>>(d.Processor), c.Resolve<Func<ISchedule>>(d.Schedule))
						);

						return container;
					};

					return factory;
				});
			}
		}
	}
}
