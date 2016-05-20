using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using QA.Scheduler.API.Models;
using QA.Scheduler.Core.Configuration;

namespace QA.Scheduler.Core.Service
{
	public static class ServiceRunner
	{
		public static void RunService<T>()
			where T : UnityContainerExtension
		{
			if (Environment.UserInteractive)
			{
				RunConsole<T>();
			}
			else
			{
				RunWinService<T>();
			}
		}

		private static void RunConsole<T>()
			where T : UnityContainerExtension
		{
			using (var container = new UnityContainer())
			{
				container.AddNewExtension<T>();
				container.AddNewExtension<SchedulerCoreConfiguration>();

				var descriptors = container.ResolveAll<ProcessorDescriptor>();
				var services = descriptors.Select(d => d.Service).Distinct().ToArray();
				bool isCanceled = false;
				var locker = new object();
				var runActions = new List<Action>();
				var cancelActions = new List<Action>();
				var serviceContainers = new List<IUnityContainer>();

				foreach (var service in services)
				{
					var serviceContainer = container.Resolve<Func<IUnityContainer>>(service)();
					var scheduler = serviceContainer.Resolve<IScheduler>();
					serviceContainers.Add(serviceContainer);
					runActions.Add(scheduler.Start);
					cancelActions.Add(scheduler.Stop);
				}

				Action stop = () =>
				{
					lock (locker)
					{
						if (!isCanceled)
						{
							Parallel.Invoke(cancelActions.ToArray());
							serviceContainers.ForEach(c => c.Dispose());
							isCanceled = true;
						}
					}
				};

				Console.CancelKeyPress += (s, e) =>
				{
					stop();
					e.Cancel = true;
				};

				Parallel.Invoke(runActions.ToArray());
				Console.ReadLine();
				stop();
			}
		}

		private static void RunWinService<T>()
			where T : UnityContainerExtension
		{
			var container = new UnityContainer();
			container.AddNewExtension<T>();
			container.AddNewExtension<SchedulerCoreConfiguration>();
			container.AddNewExtension<ServiceCoreConfiguration>();

			var services = container.Resolve<ServiceBase[]>();
			ServiceBase.Run(services);
		}
	}
}
