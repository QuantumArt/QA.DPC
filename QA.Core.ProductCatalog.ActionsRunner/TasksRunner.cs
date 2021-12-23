using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Threading;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class TasksRunner : ITasksRunner
    {
        public TasksRunner(Func<string, int, ITask> taskFactoryMethod, Func<ITaskService> taskServiceFactoryMethod, IIdentityProvider identityProvider, TaskRunnerDelays delays, IFactory consolidationFactory)
        {
            State = StateEnum.Stopped;

            Throws.IfArgumentNull(_ => taskFactoryMethod);

            Throws.IfArgumentNull(_ => taskServiceFactoryMethod);

            _taskServiceFactoryMethod = taskServiceFactoryMethod;

            _taskFactoryMethod = taskFactoryMethod;
            
            _identityProvider = identityProvider;

            _delays = delays;

            _consolidationFactory = consolidationFactory;
    }

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
        private readonly IFactory _consolidationFactory;

        public void Run(object customerCode)
        {
            _logger.Info()
                .Message("Run called for {customerCode}", customerCode)
                .Property("taskRunnerId", GetHashCode())
                .Write();

            _identityProvider.Identity = new Identity(customerCode as string);                    
            
            if (State != StateEnum.Stopped)
                throw new Exception("Already running");

            State = StateEnum.Running;

            do
            {
                try
                {
                    using (var taskService = _taskServiceFactoryMethod())
                    {
                        if (_logger.IsTraceEnabled)
                        {
                            _logger.Trace()
                                .Message("Receiving new task for processing...")
                                .Property("taskRunnerId", GetHashCode())
                                .Write();                 
                        }
                        
                        int? taskIdToRun;

                        try
                        {
                            taskIdToRun = taskService.TakeNewTaskForProcessing();
                        }
                        catch (Exception ex)
                        {
                            _logger.Error().Exception(ex)
                                .Message("Error while receiving new task for processing")
                                .Property("taskRunnerId", GetHashCode())                                
                                .Write();

                            Thread.Sleep(_delays.MsToSleepIfNoDbAccess);
                            continue;
                        }


                        if (!taskIdToRun.HasValue)
                        {
                            if (State != StateEnum.WaitingToStop)
                            {
                                if (_logger.IsTraceEnabled)
                                {
                                    _logger.Trace()
                                        .Message(
                                            "No tasks found. Sleeping for {delay} ms", _delays.MsToSleepIfNoTasks
                                        )
                                        .Property("taskRunnerId", GetHashCode())
                                        .Write();                         
                                }
                                Thread.Sleep(_delays.MsToSleepIfNoTasks);
                            }

                        }
                        else
                        {
                            _logger.Info()
                                .Message(
                                    "Task {taskId} has been started for customerCode {customerCode}", 
                                    taskIdToRun, customerCode
                                    )
                                .Property("taskRunnerId", GetHashCode())
                                .Write();

                            try
                            {
                                var taskFromQueueInfo = taskService.GetTask(taskIdToRun.Value);
                                ITask task = null;
                                
                                try
                                {
                                    task = _taskFactoryMethod(taskFromQueueInfo.Name, taskFromQueueInfo.UserID);
                                }
                                catch(Exception ex)
                                {
                                    var consolidationMessage = _consolidationFactory.Validate(customerCode as string);

                                    if (consolidationMessage == null)
                                    {
                                        throw ex;
                                    }
                                    else
                                    {
                                        throw new Exception(consolidationMessage);
                                    }
                                }

                                if (task == null)
                                    throw new Exception(
                                        $"ITask for task {taskIdToRun} with name '{taskFromQueueInfo.Name}' not found");

                                var executionContext = new ExecutionContext(this, taskIdToRun.Value);

                                StopTask = () => taskService.Cancel(taskFromQueueInfo.ID);

                                try
                                {
                                    task.Run(taskFromQueueInfo.Data, taskFromQueueInfo.Config, taskFromQueueInfo.BinData, executionContext);
                                }
                                finally
                                { 
                                    StopTask = null;
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
                                
                                _logger.Info()
                                    .Message(
                                        "Task {taskId} state has been changed to {state}", 
                                        taskIdToRun.Value, state
                                    )
                                    .Property("taskRunnerId", GetHashCode())
                                    .Write();    

                                if (state == ActionsRunnerModel.State.Done)
                                {
                                    _logger.Info()
                                        .Message(
                                            "Task {taskId} for customer code {customerCode} successfully completed", 
                                            taskIdToRun, customerCode
                                        )
                                        .Property("taskRunnerId", GetHashCode())
                                        .Property("taskResult", executionContext.Result?.ToString())
                                        .Write();                                
                                }
                                else if (state == ActionsRunnerModel.State.Failed)
                                {
                                    _logger.Info()
                                        .Message("Task {taskId} for customer code {customerCode} failed",
                                            taskIdToRun, customerCode)
                                        .Property("taskRunnerId", GetHashCode())
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

                                _logger.Error()
                                    .Exception(ex)
                                    .Message("Task {taskId} failed", taskIdToRun)
                                    .Property("taskRunnerId", GetHashCode())
                                    .Write();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error()
                        .Exception(ex)
                        .Message("General service error")
                        .Property("taskRunnerId", GetHashCode())
                        .Write();

                    InitStop();
                }
            } while (State != StateEnum.WaitingToStop);

            State = StateEnum.Stopped;
            
            _logger.Info()
                .Message("Run stopped for {customerCode}", customerCode)
                .Property("taskRunnerId", GetHashCode())
                .Write();
        }

        public void InitStop()
        {
            if (State == StateEnum.Running)
            {
                _logger.Info()
                    .Message("Run InitStop")
                    .Property("taskRunnerId", GetHashCode())
                    .Write();
                
                State = StateEnum.WaitingToStop;
                StopTask?.Invoke();
            }
        }
    }
}

