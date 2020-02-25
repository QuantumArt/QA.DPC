using NLog;
using NLog.Fluent;
using QA.Core.ProductCatalog.ActionsRunner;
using Quartz;
using Unity.Injection;
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
			
			_logger.Info()
				.Message("Calling task {taskId} by schedule.", id)
				.Property("scheduleTime", context.ScheduledFireTimeUtc)
				.Property("fireTime", context.FireTimeUtc)
				.Write();

			_service.SpawnTask(id);
			return Task.CompletedTask;
		}
	}

}
