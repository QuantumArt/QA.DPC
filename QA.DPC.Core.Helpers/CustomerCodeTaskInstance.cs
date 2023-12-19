using QA.Core.DPC.QP.Services;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Threading;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeTaskInstance
    {
        public ITaskService TaskService { get; set; }

        public ITasksRunner TasksRunner { get; set; }

        public ITask ReindexAllTask { get; set; }

        public Func<ITaskService> TaskServiceAccessor { get; set; }

        public Func<string, int, ITask> ReindexAllTaskAccessor { get; set; }

        public CustomerCodeTaskInstance(IIdentityProvider provider, ITask reindexAllTask, TaskRunnerDelays delays, IFactory consolidationFactory)
        {
            ReindexAllTask = reindexAllTask;
            ReindexAllTaskAccessor = (s, i) => s == "ReindexAllTask" ? ReindexAllTask : null;
            TaskService = new InmemoryTaskService();
            TaskServiceAccessor = () => TaskService;
            TasksRunner = new TasksRunner(ReindexAllTaskAccessor, TaskServiceAccessor, provider, delays, consolidationFactory);
            ThreadPool.QueueUserWorkItem(TasksRunner.Run, provider.Identity.CustomerCode);
        }

    }
}