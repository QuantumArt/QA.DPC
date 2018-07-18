using System;
using System.ServiceProcess;
using QA.Scheduler.API.Models;
using Unity;

namespace QA.Scheduler.Core.Service
{
    internal sealed class SchedulerService : ServiceBase
	{
		private readonly Func<IUnityContainer> _getContainer;
		private IUnityContainer _container;
		private IScheduler _scheduler;

		public SchedulerService(Func<IUnityContainer> getContainer, ServiceDescriptor descriptor)
		{
			_getContainer = getContainer;
			ServiceName = descriptor.Key;
		}

		protected override void OnStart(string[] args)
		{
			_container = _getContainer();
			_scheduler = _container.Resolve<IScheduler>();
			_scheduler.Start();
		}

		protected override void OnStop()
		{
			_scheduler.Stop();
			_container.Dispose();
			_container = null;
			_scheduler = null;
		}
	}
}
