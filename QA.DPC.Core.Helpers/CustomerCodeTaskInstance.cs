using System;
using System.Threading;
using QA.Core;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.Infrastructure;

namespace QA.DPC.Core.Helpers
{
    public class CustomerCodeTaskInstance
    {
        public ITaskService TaskService { get; set; }

        public ITasksRunner TasksRunner { get; set; }

        public ITask ReindexAllTask { get; set; }

        public Func<ITaskService> TaskServiceAccessor { get; set; }

        public Func<string, int, ITask> ReindexAllTaskAccessor { get; set; }

        public CustomerCodeTaskInstance(IIdentityProvider provider, ITask reindexAllTask, ILogger logger)
        {
            ReindexAllTask = reindexAllTask;
            ReindexAllTaskAccessor = (s, i) => s == "ReindexAllTask" ? ReindexAllTask : null;
            TaskService = new InmemoryTaskService();
            TaskServiceAccessor = () => TaskService;

            TasksRunner = new TasksRunner(ReindexAllTaskAccessor, TaskServiceAccessor, logger,
                provider, new TaskRunnerDelays());

            var actionRunnerThread = new Thread(TasksRunner.Run);
            actionRunnerThread.Start(provider.Identity.CustomerCode);
        }

    }
}