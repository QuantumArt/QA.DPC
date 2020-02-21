using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    public class TaskJob : IJob
    {
	    private readonly ITaskService _service;
		public TaskJob(ITaskService service)
		{
			_service = service;
		}

		public Task Execute(IJobExecutionContext context)
		{
			var dataMap = context.JobDetail.JobDataMap;
			var id = dataMap.GetIntValue("SourceTaskId");
			_service.SpawnTask(id);
			return Task.CompletedTask;
		}
	}

}
