﻿using QA.Core.ProductCatalog.ActionsRunnerModel;
using Quartz;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    class TaskJob : IJob
	{
		public int SourceTaskId { private get; set; }

		public void Execute(IJobExecutionContext context)
		{
			using (var taskService = ObjectFactoryBase.Resolve<ITaskService>())
			{
				taskService.SpawnTask(SourceTaskId);
			}
		}
	}

}
