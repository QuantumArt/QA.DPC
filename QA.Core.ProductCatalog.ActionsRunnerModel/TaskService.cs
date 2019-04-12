﻿using QA.Core.ProductCatalog.ActionsRunnerModel.EntityModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
 using QA.Core.DPC.QP.Services;

 namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class TaskService : ITaskService
    {
        private readonly IConnectionProvider _provider;
        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="connectionString">Строка подключения к TaskRunnerEntities.
        public TaskService(IConnectionProvider provider)
        {
            _provider = provider;
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

            using (var context = TaskRunnerEntities.Create(_provider))
            {
                return AddTask(context, task);
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

        /// <summary>
        /// получение id задачи на исполнение, то что одним запросом получается id ожидающей выполнения и апдейтится как выполняющая гарантирует
        ///что не будет конфликтов если во много потоков будет выполняться или вообще с разных машин в кластере
        /// </summary>
        /// <returns></returns>
        public int? TakeNewTaskForProcessing()
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            using (var tr = context.Database.BeginTransaction())
            {
                var task = context.Tasks.FromSql($@"SELECT TOP 1 * FROM Tasks WITH (ROWLOCK, UPDLOCK)
                WHERE StateID = {State.New} AND (
                    ExclusiveCategory IS NULL OR NOT EXISTS (
                        SELECT * FROM Tasks t
                        WHERE t.ExclusiveCategory = Tasks.ExclusiveCategory AND t.StateID = {State.Running}
                    )
                )
                ORDER BY ID").SingleOrDefault();

                if (task != null)
                {
                    context.Database.ExecuteSqlCommand($@"UPDATE Tasks
                        SET StateID = {State.Running},
                        LastStatusChangeTime = GETDATE(),
                        Progress = 0
                        WHERE ID = {task.ID}");
                }

                tr.Commit();
                return task?.ID;
            }
        }

        /// <summary>
        /// Получить задачу по ИД
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Null, если задача не найдена</returns>
        public Task GetTask(int id)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                return GetTask(context, id);
            }
        }
        
        public Task GetTask(TaskRunnerEntities ctx, int id)
        {
            return ctx.Tasks.SingleOrDefault(x => x.ID == id);
        }


        public TaskModel GetTask(int id, Expression<Func<Task, TaskModel>> selector)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                var query = context.Tasks.Where(x => x.ID == id).Select(selector);
                return query.FirstOrDefault();
            }
        }


        public bool ChangeTaskState(int id, State state, string message, State[] allowedInitialStates = null)
        {
            string sql = @"UPDATE Tasks 
                           SET StateID=@StateId, LastStatusChangeTime=GETDATE(), Message=ISNULL(@Message,Message), Progress=@Progress, IsCancellationRequested = 0 
                           WHERE ID=@ID";

            byte? progress = null;

            if (state == State.Done)
                progress = 100;
            else if (state == State.Running)
                progress = 0;

            if (allowedInitialStates != null && allowedInitialStates.Length > 0)
                sql += string.Format(" AND StateID IN ({0})",
                    string.Join(",", allowedInitialStates.Select(x => (byte) x)));
            
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                int rowsAffected = context.Database.ExecuteSqlCommand(sql,
                    new SqlParameter("@StateId", state),
                    new SqlParameter("@ID", id),
                    new SqlParameter("@Message", (object) message ?? DBNull.Value),
                    new SqlParameter("@Progress", (object) progress ?? DBNull.Value));

                return rowsAffected == 1;
            }
        }

        public Task GetLastTask(int? userId, State? state = null, string key = null)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                return
                    context.Tasks.Include("TaskState")
                        .OrderByDescending(x => x.ID)
                        .FirstOrDefault(
                            x => (x.UserID == userId || !userId.HasValue)
                                 && (x.StateID == (byte?) state || !state.HasValue)
                                 && (x.Name == key || key == null));
            }
        }

        public bool Cancel(int id)
        {
            //если еще не начали исполнять то можно сразу в отмененную
            bool cancelledWhileNew = ChangeTaskState(id, State.Cancelled, string.Empty, new[] {State.New});

            if (cancelledWhileNew)
                return true;

            //если уже выполняется то максимум можно запросить отмену
            return RequestCancelation(id);
        }

        public bool Rerun(int id)
        {
            return ChangeTaskState(id, State.New, string.Empty, new[] {State.Cancelled, State.Done, State.Failed});
        }

        public void SpawnTask(int sourceTaskId)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                var sourceTask = GetTask(context, sourceTaskId);

                if (sourceTask == null)
                    throw new ArgumentException("Task id=" + sourceTaskId + " not found");

                var task = GetTaskObject(
                    sourceTask.Name, sourceTask.Data, sourceTask.UserID, sourceTask.UserName,
                    sourceTask.DisplayName, sourceTaskId);

                AddTask(context, task);                
            }

        }

        public Task[] GetScheduledTasks()
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                return context.Tasks.Include("Schedule").Where(x => x.Schedule.Enabled).ToArray();
            }
        }

        public void SaveSchedule(int taskId, bool enabled, string cronExpression)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
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

                    task.Schedule.CronExpression = cronExpression;
                }

                context.SaveChanges();
            }
        }


        public Dictionary<int, string> GetAllStates()
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                return context.TaskStates.ToDictionary(x => x.ID, x => x.Name);
            }
        }

        public Task[] GetTasks(int skip, int take, int? userIdToFilterBy, int? stateIdToFilterBy, string nameFillter,
            bool? hasSchedule, out int totalCount)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                IQueryable<Task> tasksFiltered = context.Tasks.Include("TaskState").Include("Schedule");

                if (userIdToFilterBy.HasValue)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.UserID == userIdToFilterBy);
                }

                if (stateIdToFilterBy.HasValue)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.StateID == stateIdToFilterBy);
                }

                if (nameFillter != null)
                {
                    tasksFiltered = tasksFiltered.Where(x => x.DisplayName.Contains(nameFillter));
                }

                if (hasSchedule.HasValue)
                {
                    if (hasSchedule.Value)
                    {
                        tasksFiltered = tasksFiltered.Where(x => x.Schedule.Enabled == true);
                    }
                    else
                    {
                        tasksFiltered = tasksFiltered.Where(x => x.Schedule == null || x.Schedule.Enabled == false);
                    }
                }

                totalCount = tasksFiltered.Count();

                return tasksFiltered
                    .OrderByDescending(x => x.ID)
                    .Skip(skip)
                    .Take(take)
                    .ToArray();               
            }

        }


        public bool RequestCancelation(int id)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                int rowsAffected = context.Database.ExecuteSqlCommand(
                    "UPDATE Tasks SET IsCancellationRequested = 1 WHERE ID=@ID AND StateID=@RunningStateId",
                    new SqlParameter("@RunningStateId", State.Running),
                    new SqlParameter("@ID", id));

                return rowsAffected == 1;
            }
        }

        public void SetTaskProgress(int id, byte progress)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                context.Database.ExecuteSqlCommand(
                    "UPDATE Tasks SET Progress=@Progress WHERE ID=@ID",
                    new SqlParameter("@Progress", progress), new SqlParameter("@ID", id));
            }
        }

        public bool GetIsCancellationRequested(int taskId)
        {
            using (var context = TaskRunnerEntities.Create(_provider))
            {
                return
                    context.Tasks.FromSql($"SELECT * FROM Tasks WITH (nolock) WHERE ID = {taskId}", taskId)
                        .SingleOrDefault()?.IsCancellationRequested ?? false;
            }
        }

        public void Dispose()
        {

        }
    }
}