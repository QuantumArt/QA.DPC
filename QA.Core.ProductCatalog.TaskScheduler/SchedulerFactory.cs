using Quartz;
using Quartz.Core;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;
using Quartz.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    public class SchedulerFactory : ISchedulerFactory
	{
		private ConcurrentDictionary<string, IScheduler> _schedulerMap;

		public SchedulerFactory()
		{
			_schedulerMap = new ConcurrentDictionary<string, IScheduler>();
		}

		private static IScheduler CreateScheduler(string customerCode)
		{
			var ramJobStore = new RAMJobStore();

			var schedulerResources = new QuartzSchedulerResources
			{
				JobRunShellFactory = new StdJobRunShellFactory(),
				JobStore = ramJobStore,
				Name = $"TasksScheduler_{customerCode}"
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
			IReadOnlyList<IScheduler> schedulers = _schedulerMap.Values.ToList();
			return Task.FromResult(schedulers);
		}

		public Task<IScheduler> GetScheduler(CancellationToken cancellationToken = new CancellationToken())
		{
			throw new NotImplementedException();
		}

		public Task<IScheduler> GetScheduler(string schedName, CancellationToken cancellationToken = new CancellationToken())
		{
			var scheduler = _schedulerMap.GetOrAdd(schedName, CreateScheduler);
			return Task.FromResult(scheduler);
		}
	}
}
