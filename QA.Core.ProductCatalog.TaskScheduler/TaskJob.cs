﻿using NLog;
using QA.Core.ProductCatalog.ActionsRunner;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    public class TaskJob : IJob
    {
	    private readonly ITaskService _service;

	    private static ILogger _logger = LogManager.GetCurrentClassLogger();
	    
	    
		public TaskJob(ITaskService service)
		{
			_service = service;
		}

		public Task Execute(IJobExecutionContext context)
		{
			var dataMap = context.JobDetail.JobDataMap;
			var id = dataMap.GetIntValue("SourceTaskId");
			
			_logger.ForInfoEvent()
				.Message("Calling task {taskId} by schedule.", id)
				.Property("scheduleTime", context.ScheduledFireTimeUtc)
				.Property("fireTime", context.FireTimeUtc)
				.Log();

			_service.SpawnTask(id);
			return Task.CompletedTask;
		}
	}

}
