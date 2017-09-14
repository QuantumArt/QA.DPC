using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using QA.Scheduler.API.Services;
using QA.Scheduler.API.Models;
using QA.Scheduler.Core.Schedules;
using QA.Core;
using QA.Core.Logger;

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
				Container.RegisterType<Func<IUnityContainer>>(service, new InjectionFactory(parent =>
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

						container.RegisterType<ILogger>(new InjectionFactory(c => c.Resolve<ILogger>(service)));
						container.RegisterType<ServiceDescriptor>(new InjectionFactory(c => c.Resolve<ServiceDescriptor>(service)));
						container.RegisterType<IScheduler, Scheduler>(new HierarchicalLifetimeManager(), new InjectionFactory(c => new Scheduler(c.Resolve<IEnumerable<IProcessor>>(), c.Resolve<ILogger>())));
						Container.RegisterType<IEnumerable<IProcessor>>(new InjectionFactory(c =>
							from d in descriptors
							where d.Service == service
							select new ScheduledProcessor(c.Resolve<Func<IProcessor>>(d.Processor), c.Resolve<Func<ISchedule>>(d.Schedule))
						));

						return container;
					};

					return factory;
				}));
			}
		}
	}
}
