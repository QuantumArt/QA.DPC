using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using NLog;
using NLog.Fluent;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Threading;
using Newtonsoft.Json;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class TasksRunner : ITasksRunner
    {
        public TasksRunner(Func<string, int, ITask> taskFactoryMethod, Func<ITaskService> taskServiceFactoryMethod, IIdentityProvider identityProvider, TaskRunnerDelays delays)
        {
            State = StateEnum.Stopped;

            Throws.IfArgumentNull(_ => taskFactoryMethod);

            Throws.IfArgumentNull(_ => taskServiceFactoryMethod);

            _taskServiceFactoryMethod = taskServiceFactoryMethod;

            _taskFactoryMethod = taskFactoryMethod;
            
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

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        
        private readonly IIdentityProvider _identityProvider;
        private readonly Func<string, int, ITask> _taskFactoryMethod;
        private readonly Func<ITaskService> _taskServiceFactoryMethod;
        private readonly TaskRunnerDelays _delays;

        public void Run(object customerCode)
        {
            _logger.Info().Message("Run called for {customerCode}", customerCode).Write();

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
                            _logger.Error().Exception(ex)
                                .Message("Error getting task Id to process")
                                .Write();

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
                            _logger.Info()
                                .Message(
                                    "Task {taskId} has been started for customerCode {customerCode}", 
                                    taskIdToRun, customerCode
                                    )
                                .Write();

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

                                try
                                {
                                    task.Run(taskFromQueueInfo.Data, taskFromQueueInfo.Config, taskFromQueueInfo.BinData, executionContext);
                                }
                                finally
                                {
                                    lock (_stateLoker)
                                    {
                                        StopTask = null;
                                    }
                                }

                                var state = executionContext.IsCancelled
                                    ? ActionsRunnerModel.State.Cancelled
                                    : (executionContext.Result.IsSuccess
                                        ? ActionsRunnerModel.State.Done
                                        : ActionsRunnerModel.State.Failed
                                    );

                                taskService.ChangeTaskState(
                                    taskIdToRun.Value, 
                                    state, 
                                    JsonConvert.SerializeObject(executionContext.Result)
                                );

                                if (state == ActionsRunnerModel.State.Done)
                                {
                                    _logger.Info()
                                        .Message(
                                            "Task {taskId} for customer code {customerCode} successfully completed", 
                                            taskIdToRun, customerCode, executionContext.Result.ToString()
                                        )
                                        .Property("taskResult", executionContext.Result?.ToString())
                                        .Write();                                
                                }
                                else if (state == ActionsRunnerModel.State.Failed)
                                {
                                    _logger.Error()
                                        .Message("Task {taskId} for customer code {customerCode} failed", taskIdToRun)
                                        .Property("taskResult", executionContext.Result?.ToString())
                                        .Write();                                 
                                }
                            }
                            catch (Exception ex)
                            {
                                string errMessage = ex.Message;

                                if (ex.InnerException != null)
                                    errMessage += " " + ex.InnerException.Message;

                                taskService.ChangeTaskState(taskIdToRun.Value, ActionsRunnerModel.State.Failed, errMessage);

                                _logger.Error().Exception(ex)
                                    .Message("Task {taskId} failed", taskIdToRun)
                                    .Write();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error().Exception(ex)
                        .Message("General service error");

                    InitStop();
                }
            } while (State != StateEnum.WaitingToStop);

            _logger.Info().Message("Run stopped for {customerCode}", customerCode)
                .Write();
            State = StateEnum.Stopped;
        }

        public void InitStop()
        {
            lock (_stateLoker)
            {
                if (State == StateEnum.Running)
                {
                    _logger.Info().Message("Run init stop")
                        .Write();
                    State = StateEnum.WaitingToStop;
                    StopTask?.Invoke();
                }
            }
        }
    }
}

