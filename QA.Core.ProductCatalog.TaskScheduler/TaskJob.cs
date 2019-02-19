using QA.Core.ProductCatalog.ActionsRunnerModel;
using Quartz;
using Task = System.Threading.Tasks.Task;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    public class TaskJob : IJob
    {
	    private ITaskService _service;
		public TaskJob(ITaskService service)
		{
			_service = service;
		}
		
		public int SourceTaskId { private get; set; }

		public Task Execute(IJobExecutionContext context)
		{
			_service.SpawnTask(SourceTaskId);
			return Task.CompletedTask;
		}
	}

}
