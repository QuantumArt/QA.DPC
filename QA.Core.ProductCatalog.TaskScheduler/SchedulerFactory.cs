using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Core;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;
using Quartz.Util;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    public class SchedulerFactory : ISchedulerFactory
    {

	    private IScheduler _scheduler;

	    public SchedulerFactory()
	    {
		    _scheduler = CreateScheduler();
	    }
		
		public ICollection<IScheduler> AllSchedulers => new[] {_scheduler};

		public IScheduler GetScheduler(string schedName)
		{
			return _scheduler;
		}

		public IScheduler GetScheduler()
		{
			return _scheduler;
		}

		private static IScheduler CreateScheduler()
		{
			var ramJobStore = new RAMJobStore();

			var schedulerResources = new QuartzSchedulerResources
			{
				JobRunShellFactory = new StdJobRunShellFactory(),
				JobStore = ramJobStore,
				Name = "TasksScheduler"
			};

			var threadPool = new DefaultThreadPool();

			threadPool.Initialize();

			schedulerResources.ThreadPool = threadPool;

			var quartzScheduler = new QuartzScheduler(schedulerResources, TimeSpan.Zero);

			ITypeLoadHelper loadHelper;
			try
			{
				loadHelper = ObjectUtils.InstantiateType<ITypeLoadHelper>(typeof(SimpleTypeLoadHelper));
			}
			catch (Exception e)
			{
				throw new SchedulerConfigException("Unable to instantiate type load helper: {0}".FormatInvariant(e.Message), e);
			}

			loadHelper.Initialize();

			ramJobStore.Initialize(loadHelper, quartzScheduler.SchedulerSignaler);

			var standartScheduler = new StdScheduler(quartzScheduler);

			schedulerResources.JobRunShellFactory.Initialize(standartScheduler);

			quartzScheduler.Initialize();

			SchedulerRepository schedRep = SchedulerRepository.Instance;

			schedRep.Bind(standartScheduler);

			return standartScheduler;
		}

		public Task<IReadOnlyList<IScheduler>> GetAllSchedulers(CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<IScheduler> GetScheduler(CancellationToken cancellationToken = new CancellationToken())
		{
			return Task.FromResult(_scheduler);
		}

		public Task<IScheduler> GetScheduler(string schedName, CancellationToken cancellationToken = new CancellationToken())
		{
			return Task.FromResult(_scheduler);
		}
	}

}
