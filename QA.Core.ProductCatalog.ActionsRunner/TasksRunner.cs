using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Threading;

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
        public Action StopTask { get; private set; }

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
            _logger?.Info($"Run called for {customerCode}");

            _identityProvider.Identity = new Identity(customerCode as string);                    

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
                            _logger?.Info("Задача {0} принята в работу для {1}", taskIdToRun, customerCode);

                            try
                            {
                                var taskFromQueueInfo = taskService.GetTask(taskIdToRun.Value);

                                var task = _taskFactoryMethod(taskFromQueueInfo.Name, taskFromQueueInfo.UserID);

                                if (task == null)
                                    throw new Exception(
                                        $"ITask for task {taskIdToRun} with name '{taskFromQueueInfo.Name}' not found");

                                var executionContext = new ExecutionContext(this, taskIdToRun.Value);

                                lock (_stateLoker)
                                {
                                    StopTask = () => taskService.Cancel(taskFromQueueInfo.ID);
                                }

                                task.Run(taskFromQueueInfo.Data, taskFromQueueInfo.Config, taskFromQueueInfo.BinData, executionContext);

                                lock (_stateLoker)
                                {
                                    StopTask = null;
                                }

                                taskService.ChangeTaskState(taskIdToRun.Value, executionContext.IsCancelled ? ActionsRunnerModel.State.Cancelled : ActionsRunnerModel.State.Done, executionContext.Message);

                                _logger?.Info("Задача {0} для {1} успешно выполнилась. Сообщение: {2}", taskIdToRun, customerCode, executionContext.Message);
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

            _logger?.Info($"Run stopped for {customerCode}");
            State = StateEnum.Stopped;
        }

        public void InitStop()
        {
            lock (_stateLoker)
            {
                if (State == StateEnum.Running)
                {
                    _logger?.Info($"Run init stop");
                    State = StateEnum.WaitingToStop;
                    StopTask?.Invoke();
                }
            }
        }
    }
}

