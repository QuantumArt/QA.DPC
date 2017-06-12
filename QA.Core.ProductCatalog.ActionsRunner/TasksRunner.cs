using System;
using System.Diagnostics;
using System.Threading;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.Infrastructure;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class TasksRunner : ITasksRunner
    {
        public TasksRunner(Func<string, int, ITask> taskFactoryMethod, Func<ITaskService> taskServiceFactoryMethod, ILogger logger, IIdentityProvider identityProvider, TaskRunnerDelays delays)
        {
            State = StateEnum.Stopped;

            Throws.IfArgumentNull(_ => taskFactoryMethod);

            Throws.IfArgumentNull(_ => taskServiceFactoryMethod);

            _taskServiceFactoryMethod = taskServiceFactoryMethod;

            _taskFactoryMethod = taskFactoryMethod;
            
            _logger = logger;

            _identityProvider = identityProvider;

            _delays = delays;
        }

        private readonly object _stateLoker = new object();

        public StateEnum State { get; private set; }

        public void SetTaskProgress(int taskId, byte progress)
        {
            if (progress > 100)
                throw new ArgumentOutOfRangeException(nameof(progress));

            using (var taskService = _taskServiceFactoryMethod())
            {
                taskService.SetTaskProgress(taskId, progress);
            }
        }

        public bool GetIsCancellationRequested(int taskId)
        {
            using (var taskService = _taskServiceFactoryMethod())
            {
                return taskService.GetIsCancellationRequested(taskId);
            }
        }

        private readonly ILogger _logger;
        
        private readonly IIdentityProvider _identityProvider;
        private readonly Func<string, int, ITask> _taskFactoryMethod;
        private readonly Func<ITaskService> _taskServiceFactoryMethod;
        private readonly TaskRunnerDelays _delays;

        public void Run(object customerCode)
        {
            _identityProvider.Identity = new Identity(customerCode as string);

            _logger?.Info("Run called");

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            lock (_stateLoker)
            {
                if (State != StateEnum.Stopped)
                    throw new Exception("Already running");

                State = StateEnum.Running;
            }

            do
            {
                try
                {
                    using (var taskService = _taskServiceFactoryMethod())
                    {
                        int? taskIdToRun;

                        try
                        {
                            taskIdToRun = taskService.TakeNewTaskForProcessing();
                        }
                        catch (Exception ex)
                        {
                            _logger?.ErrorException("Error getting task ID to process", ex);

                            Thread.Sleep(_delays.MsToSleepIfNoDbAccess);
                            continue;
                        }


                        if (!taskIdToRun.HasValue)
                        {
                            if (State != StateEnum.WaitingToStop)
                                Thread.Sleep(_delays.MsToSleepIfNoTasks);
                        }
                        else
                        {
                            _logger?.Info("Задача {0} принята в работу", taskIdToRun);

                            try
                            {
                                var taskFromQueueInfo = taskService.GetTask(taskIdToRun.Value);

                                var task = _taskFactoryMethod(taskFromQueueInfo.Name, taskFromQueueInfo.UserID);

                                if (task == null)
                                    throw new Exception(
                                        $"ITask for task {taskIdToRun} with name '{taskFromQueueInfo.Name}' not found");

                                var executionContext = new ExecutionContext(this, taskIdToRun.Value);
                                task.Run(taskFromQueueInfo.Data, taskFromQueueInfo.Config, taskFromQueueInfo.BinData, executionContext);

                                taskService.ChangeTaskState(taskIdToRun.Value, executionContext.IsCancelled ? ActionsRunnerModel.State.Cancelled : ActionsRunnerModel.State.Done, executionContext.Message);

                                _logger?.Info("Задача {0} успешно выполнилась. Сообщение: {1}", taskIdToRun, executionContext.Message);
                            }
                            catch (Exception ex)
                            {
                                string errMessage = ex.Message;

                                if (ex.InnerException != null)
                                    errMessage += " " + ex.InnerException.Message;

                                taskService.ChangeTaskState(taskIdToRun.Value, ActionsRunnerModel.State.Failed, errMessage);

                                _logger?.ErrorException("Задача {0} не выполнилась", ex, taskIdToRun);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.ErrorException("Общая ошибка в работе сервиса", ex);

                    InitStop();
                }
            } while (State != StateEnum.WaitingToStop);

            State = StateEnum.Stopped;


        }

        public void InitStop()
        {
            lock (_stateLoker)
            {
                if (State == StateEnum.Running)
                    State = StateEnum.WaitingToStop;
            }
        }
    }
}

