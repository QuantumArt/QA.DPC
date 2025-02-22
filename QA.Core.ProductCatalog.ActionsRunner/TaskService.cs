﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using M = QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using QP.ConfigurationService.Models;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public class TaskService : ITaskService
    {
        private readonly IConnectionProvider _provider;
        private readonly M.Customer _customer;
        private readonly ResourceManager _taskRm;
        private readonly static ILogger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="connectionString">Строка подключения к TaskRunnerEntities.
        public TaskService(IConnectionProvider provider)
        {
            _provider = provider;
            _customer = _provider.GetCustomer(M.Service.Actions);
            _taskRm = new ResourceManager(typeof(TaskStrings));
        }

        /// <summary>
        /// создает задачу
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="taskDisplayName">имя задачи для UI</param>
        /// <param name="sourceTaskId">если создается по расписанию id исходной где расписание</param>
        /// <param name="exclusiveCategory">если не null то одновременно может выполняться ровно одна задача с таким значением</param>
        /// <param name="config">config</param>
        /// <returns></returns>
        
        public int AddTask(string key, string data, int userId, string userName, string taskDisplayName,
        int? sourceTaskId = null, string exclusiveCategory = null, string config = null, byte[] binData = null)
        {
            var task = GetTaskObject(key, data, userId, userName, taskDisplayName, sourceTaskId, exclusiveCategory, config, binData);
            
            Logger.ForInfoEvent()
                .Message("Adding task...")
                .Property("task", task)
                .Log();
            
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                var result = AddTask(context, task);
                
                Logger.ForInfoEvent()
                    .Message("Task {taskId} added", result)
                    .Log();
                
                return result;
            }
        }

        private static Task GetTaskObject(string key, string data, int userId, string userName, string taskDisplayName,
            int? sourceTaskId = null, string exclusiveCategory = null, string config = null, byte[] binData = null)
        {
            var task = new Task
            {
                Name = key,
                Data = data,
                CreatedTime = DateTime.Now,
                StateID = (byte) State.New,
                UserID = userId,
                UserName = userName ?? string.Empty,
                DisplayName = taskDisplayName,
                ScheduledFromTaskID = sourceTaskId,
                ExclusiveCategory = exclusiveCategory,
                Config = config,
                BinData = binData
            };
            return task;
        }

        public int AddTask(TaskRunnerEntities ctx, Task task)
        {
            ctx.Tasks.Add(task);
            ctx.SaveChanges();
            return task.ID;
        }

        private string _sqlForUpdateHint => _customer.DatabaseType == DatabaseType.Postgres ? "" : "WITH (ROWLOCK, UPDLOCK)";
        
        private string _sqlUpdateHint => _customer.DatabaseType == DatabaseType.Postgres ? "" : "WITH (ROWLOCK)";
        
        private string _pgForUpdateHint => _customer.DatabaseType == DatabaseType.Postgres ? "FOR UPDATE" : "";

        private string _sqlNolockHint => _customer.DatabaseType == DatabaseType.Postgres ? "" : "WITH (NOLOCK)";
        
        private string _stateId => _customer.DatabaseType == DatabaseType.Postgres ? "state_id" : "StateId";
        
        private string _exclusiveCategory => _customer.DatabaseType == DatabaseType.Postgres ? "exclusive_category" : "ExclusiveCategory";

        private string _isCancellationRequested => _customer.DatabaseType == DatabaseType.Postgres ? "is_cancellation_requested" : "IsCancellationRequested";

        private object _getIsCancellationRequestedValue(bool v) => _customer.DatabaseType == DatabaseType.Postgres ? (object)v : v ? 1 : 0;

        private string SqlTop(int n) => _customer.DatabaseType == DatabaseType.Postgres ? "" : "TOP " + n;
        
        private string PgTop(int n) => _customer.DatabaseType == DatabaseType.Postgres ? $"\nFETCH FIRST {n} ROWS ONLY" : "";
        
        
        

        /// <summary>
        /// получение id задачи на исполнение, то что одним запросом получается id ожидающей выполнения и апдейтится как выполняющая гарантирует
        ///что не будет конфликтов если во много потоков будет выполняться или вообще с разных машин в кластере
        /// </summary>
        /// <returns></returns>
        public int? TakeNewTaskForProcessing()
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            using (var tr = context.Database.BeginTransaction())
            {
                var sql =
                $@"SELECT {SqlTop(1)} * FROM tasks {_sqlForUpdateHint}
                WHERE {_stateId} = {{0}} AND (
                    {_exclusiveCategory} IS NULL OR NOT EXISTS (
                        SELECT * FROM tasks t
                        WHERE t.{_exclusiveCategory} = tasks.{_exclusiveCategory} AND t.{_stateId} = {{1}}
                    )
                )
                ORDER BY ID {_pgForUpdateHint} {PgTop(1)}";
                var task = context.Tasks.FromSqlRaw(sql, State.New, State.Running).SingleOrDefault();

                if (task == null)
                    return null;

                task.StateID = (byte) State.Running;
                task.LastStatusChangeTime = DateTime.Now;
                task.Progress = 0;
                context.SaveChanges();
                tr.Commit();
                return task.ID;
            }
        }

        public Task GetTask(int id, bool convertMessage = false)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                return GetTask(context, id, convertMessage);
            }
        }
        
        public Task GetTask(TaskRunnerEntities ctx, int id, bool convertMessage = false)
        {
            var task = ctx.Tasks.Include(x => x.TaskState).SingleOrDefault(x => x.ID == id);
            if (task != null)
            {
                if (task.ScheduleID != null)
                {
                    task.Schedule = ctx.Schedules.SingleOrDefault(n => n.ID == task.ScheduleID);
                }

                if (convertMessage)
                {
                    task.Message = MessageToDisplay(task.Message);
                } 
            }

            return task;
        }
        
        public Task GetTaskWithUpdateLock(TaskRunnerEntities ctx, int id)
        {
            var sql = $@"SELECT * FROM tasks {_sqlForUpdateHint} where id = {{0}} {_pgForUpdateHint}";
            return ctx.Tasks.FromSqlRaw(sql, id).SingleOrDefault();
        }
        
        public Task GetTaskWithNoLock(TaskRunnerEntities ctx, int id)
        {
            var sql = $@"SELECT * FROM tasks {_sqlNolockHint} where id = {{0}}";
            return ctx.Tasks.FromSqlRaw(sql, id).SingleOrDefault();
        }



        public bool ChangeTaskState(int id, State state, string message, State[] allowedInitialStates = null)
        {
            byte? progress = null;

            if (state == State.Done)
                progress = 100;
            else if (state == State.Running)
                progress = 0;

            using (var context = TaskRunnerEntities.Get(_customer))
            using (var tr = context.Database.BeginTransaction())
            {
                
                Logger.ForTraceEvent()
                    .Message("Receiving task {taskId} for updating...", id)
                    .Log();
                
                var task = GetTaskWithUpdateLock(context, id);
                if (task == null)
                {
                    Logger.ForWarnEvent()
                        .Message("Task {taskId} is not found", id)
                        .Log();
                    
                    return false;
                }

                if (allowedInitialStates != null && !allowedInitialStates.Select(x => (int) x).Contains(task.StateID))
                {
                    Logger.ForWarnEvent()
                        .Message("Task {taskId} has been excluded by state {state}", id, task.TaskState)
                        .Log();
                    
                    return false;
                }
                
                task.StateID = (int) state;
                task.Message = message ?? task.Message;
                task.Progress = progress ?? task.Progress;
                task.IsCancellationRequested = false;
                task.LastStatusChangeTime = DateTime.Now;

                context.SaveChanges();
                tr.Commit();
                
                Logger.ForTraceEvent()
                    .Message("Task {taskId} has been updated", id)
                    .Property("task", task)
                    .Log();
                
                return true;
                
            }
        }

        public Task GetLastTask(int? userId, State? state = null, string key = null)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                var intState = (int?) state;
                var result =
                    context.Tasks.Include("TaskState")
                        .OrderByDescending(x => x.ID)
                        .FirstOrDefault(
                            x => (x.UserID == userId || !userId.HasValue)
                                 && (x.StateID == intState || !intState.HasValue)
                                 && (x.Name == key || key == null));

                if (result != null)
                {
                    result.Message = MessageToDisplay(result.Message);
                    result.TaskState.Name = _taskRm.GetString(result.TaskState.Name) ?? result.TaskState.Name;
                }

                return result;
            }
        }

        public bool Cancel(int id)
        {
            //если еще не начали исполнять то можно сразу в отмененную
            bool cancelledWhileNew = ChangeTaskState(id, State.Cancelled, string.Empty, new[] {State.New});

            if (cancelledWhileNew)
                return true;

            //если уже выполняется то максимум можно запросить отмену
            return RequestCancellation(id);
        }

        public bool Rerun(int id)
        {
            return ChangeTaskState(id, State.New, string.Empty, new[] {State.Cancelled, State.Done, State.Failed});
        }

        public void SpawnTask(int sourceTaskId)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                var sourceTask = GetTask(context, sourceTaskId);

                if (sourceTask == null)
                    throw new ArgumentException("Task id=" + sourceTaskId + " not found");

                var task = GetTaskObject(
                    sourceTask.Name, sourceTask.Data, sourceTask.UserID, sourceTask.UserName,
                    sourceTask.DisplayName, sourceTaskId);

                if (CanSpawnTask(context, sourceTask))
                {
                    AddTask(context, task);
                }
            }
        }

        private bool CanSpawnTask(TaskRunnerEntities ctx, Task task)
        {
            var allowConcurrent = task.Schedule?.AllowConcurrentTasks ?? true;

            return allowConcurrent || !ctx.Tasks.Any(t => t.StateID == (int)State.New && t.ScheduledFromTaskID == task.ID);
        }

        public Task[] GetScheduledTasks()
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                return (context == null) ? new Task[] {} : 
                    context.Tasks.Include(x => x.Schedule).Where(x => x.Schedule.Enabled).ToArray();              
            }

        }

        public void SaveSchedule(int taskId, bool enabled, bool allowConcurrentTasks, string cronExpression)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                var task = GetTask(context, taskId);

                if (task == null)
                    throw new ArgumentException("Task id=" + taskId + " not found");

 
                if (string.IsNullOrEmpty(cronExpression))
                {
                    if (task.Schedule != null)
                    {
                        context.Schedules.Remove(task.Schedule);

                        task.Schedule = null;
                    }
                }
                else
                {
                    if (task.Schedule == null)
                        task.Schedule = new Schedule();

                    task.Schedule.Enabled = enabled;
                    task.Schedule.AllowConcurrentTasks = allowConcurrentTasks;
                    task.Schedule.CronExpression = cronExpression;
                }

                context.SaveChanges();               
            }

        }

        public void CancelRequestedTasks()
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                if (context == null) return;
                var sql =
                    ($@"UPDATE tasks {_sqlUpdateHint} SET {_stateId}={{0}} WHERE {_isCancellationRequested} = {{1}} AND {_stateId}={{2}}");
                var fString = FormattableStringFactory.Create(sql, State.Cancelled, _getIsCancellationRequestedValue(true), State.Running);
                context.Database.ExecuteSqlInterpolated(fString);
            }
        }


        public Dictionary<int, string> GetAllStates()
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                return context.TaskStates.ToDictionary(x => x.ID, x => x.Name);
            }
        }
        
        protected string MessageToDisplay(string message)
        {
            try
            {
                var taskResult = JsonConvert.DeserializeObject<ActionTaskResult>(message);
                return taskResult.ToString();
            }
            catch (Exception)
            {
                return message;
            }
        }

        public Task[] GetTasks(int skip, int take, int? userIdToFilterBy, int? stateIdToFilterBy, string nameFilter,
            bool? hasSchedule, DateTime? createdLower, DateTime? createdUpper, out int totalCount)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                IQueryable<Task> tasksFiltered = context.Tasks.Include(n => n.TaskState).Include(n => n.Schedule);

                if (userIdToFilterBy.HasValue)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.UserID == userIdToFilterBy);
                }

                if (stateIdToFilterBy.HasValue)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.StateID == stateIdToFilterBy);
                }
                
                if (createdLower.HasValue)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.CreatedTime >= createdLower);
                }
                
                if (createdUpper.HasValue)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.CreatedTime <= createdUpper);
                }

                if (nameFilter != null)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.DisplayName.Contains(nameFilter));
                }

                if (hasSchedule.HasValue)
                {
                    tasksFiltered = hasSchedule.Value ? tasksFiltered.Where(x => x.Schedule != null) : tasksFiltered.Where(x => x.Schedule == null);
                }

                totalCount = tasksFiltered.Count();

                var result = tasksFiltered
                    .OrderByDescending(x => x.ID)
                    .Skip(skip)
                    .Take(take)
                    .ToArray();

                foreach (var task in result)
                {
                    task.Message = MessageToDisplay(task.Message);
                    task.TaskState.Name = _taskRm.GetString(_taskStates[task.StateID]) ?? task.TaskState.Name;
                }

                return result;
            }              
        }

        private readonly Dictionary<int, string> _taskStates = new Dictionary<int, string>
        {
            {1, "New"},
            {2, "Running"},
            {3, "Completed"},
            {4, "Error"},
            {5, "Cancelled"}
        };


        private bool RequestCancellation(int id)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                var sql = ($@"UPDATE tasks SET {_isCancellationRequested} = {{0}} WHERE id = {{1}} AND {_stateId}={{2}}");
                var fString = FormattableStringFactory.Create(sql, _getIsCancellationRequestedValue(true), id, State.Running);
                var rowsAffected = context.Database.ExecuteSqlInterpolated(fString);
                return rowsAffected == 1;            
            }
        }

        public void SetTaskProgress(int id, byte progress)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                context.Database.ExecuteSqlInterpolated(
                    $@"UPDATE tasks SET progress={progress} WHERE id={id}"
                );             
            }
        }

        public bool GetIsCancellationRequested(int taskId)
        {
            using (var context = TaskRunnerEntities.Get(_customer))
            {
                var task = GetTaskWithNoLock(context, taskId);
                return task?.IsCancellationRequested ?? false;            
            }
        }

        public void Dispose()
        {

        }
    }
}