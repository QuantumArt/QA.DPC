using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.TaskScheduler;
using System.Collections.Generic;
using System.Linq;

namespace QA.Core.ProductCatalog.ActionsService
{
    public class ActionsServiceContext
    {
        public string CustomerCode { get; private set; }
        public List<ITasksRunner> Runners { get; private set; }
        public TaskSchedulerRunner SchedulerRunner { get; set; }

        public ActionsServiceContext(string customerCode)
        {
            CustomerCode = customerCode;
            Runners = new List<ITasksRunner>();
        }

        public bool IsStopped()
        {
            return Runners.All(t => t.State == StateEnum.Stopped) && (SchedulerRunner == null || !SchedulerRunner.IsRunning);
        }
    }
}
