using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Practices.Unity;
using QA.Scheduler.API.Models;
using QA.Scheduler.Core.Configuration;

namespace QA.Scheduler.Core.Service
{
	public class ServiceInstaller<T> : Installer
		where T : UnityContainerExtension
	{
		public ServiceInstaller()
		{
			//Debugger.Launch();

			using (var container = new UnityContainer())
			{
				container.AddNewExtension<T>();
				container.AddNewExtension<SchedulerCoreConfiguration>();

				var processInstaller = new ServiceProcessInstaller();
				processInstaller.Password = null;
				processInstaller.Username = null;

				var descriptors = container.Resolve<ServiceDescriptor[]>();

				var serviceInstallers = (from descriptor in descriptors
										select new ServiceInstaller
										{
											StartType = ServiceStartMode.Manual,
											DisplayName = descriptor.Name,
											ServiceName = descriptor.Key,
											Description = descriptor.Description
										})
										.ToArray();

				Installers.Add(processInstaller);
				Installers.AddRange(serviceInstallers);
			}
		}
	}
}
