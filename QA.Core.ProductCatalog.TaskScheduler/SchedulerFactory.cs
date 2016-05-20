using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		public ICollection<IScheduler> AllSchedulers
		{
			get { throw new NotImplementedException(); }
		}

		public IScheduler GetScheduler(string schedName)
		{
			throw new NotImplementedException();
		}

		public IScheduler GetScheduler()
		{
			var ramJobStore = new RAMJobStore();

			var schedulerResources = new QuartzSchedulerResources
			{
				JobRunShellFactory = new StdJobRunShellFactory(),
				JobStore = ramJobStore,
				Name = "TasksScheduler"
			};

			var threadPool = new SimpleThreadPool();

			threadPool.Initialize();

			schedulerResources.ThreadPool = threadPool;

			schedulerResources.ThreadExecutor = new DefaultThreadExecutor();

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
	}

}
