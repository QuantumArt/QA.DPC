using System;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using Quartz;
using Quartz.Spi;

namespace QA.Core.ProductCatalog.TaskScheduler
{
    public class TaskServiceJobFactory : IJobFactory
    {
        private ITaskService _service;

        public TaskServiceJobFactory(ITaskService service)
        {
            _service = service;
        }
		
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return new TaskJob(_service);
        }

        public void ReturnJob(IJob job)
        {
            throw new NotImplementedException();
        }
    }
}