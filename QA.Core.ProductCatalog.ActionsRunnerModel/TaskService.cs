using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.Configuration;
using System.Linq.Expressions;
using QA.Core.ProductCatalog.ActionsRunnerModel.EntityModels;
using Microsoft.Practices.Unity;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class TaskService : ITaskService
    {
        private readonly TaskRunnerEntities _dbContext;
        private readonly bool _useTaskRunnerMessageLengthRestriction;

        private Int32 TaskRunnerMaxMessageLength
        {
            get
            {
                int p;

                if (Int32.TryParse(WebConfigurationManager.AppSettings["TaskRunnerMaxMessageLength"], out p))
                {

                    return p;

                }  //if

                return 1024000;
            }
        }

        private Int32? CommandTimeout
        {
            get
            {
                int p;
                
                if(Int32.TryParse(WebConfigurationManager.AppSettings["TaskRunnerCommandTimeout"], out p)) {
                
                    return p;

                }  //if

                return null;
            }
        }

        public TaskService(TaskRunnerEntities dbContext) : this(dbContext, null, true)
        {

        }

        /// <summary>
        /// конструктор
        /// </summary>
        /// <param name="dbContext">Контекст TaskRunnerEntities</param>
        /// <param name="nameOrConnectionString">Строка или имя строки подключения к TaskRunnerEntities. Если указано, то обладает приоритетом над параметром <paramref name="dbContext"/></param>
        /// <param name="useTaskRunnerMessageLengthRestriction">Использовать ли ограничение на длину сообщения</param>
        public TaskService(TaskRunnerEntities dbContext, string nameOrConnectionString, bool useTaskRunnerMessageLengthRestriction)
        {
            if (nameOrConnectionString != null)
                dbContext = new TaskRunnerEntities(nameOrConnectionString);

            if (dbContext == null)
                throw new ArgumentNullException("dbContext");

            _dbContext = dbContext;
            
            if (CommandTimeout.HasValue)
            {
                _dbContext.Database.CommandTimeout = CommandTimeout;
            }  //if

            _useTaskRunnerMessageLengthRestriction = useTaskRunnerMessageLengthRestriction;
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
        public int AddTask(string key, string data, int userId, string userName, string taskDisplayName, int? sourceTaskId = null, string exclusiveCategory = null, string config = null, byte[] binData = null)
        {
            var task = new Task
            {
                Name = key,
                Data = data,
                CreatedTime = DateTime.Now,
                StateID = (byte)State.New,
                UserID = userId,
                UserName = userName ?? string.Empty,
                DisplayName = taskDisplayName,
				ScheduledFromTaskID = sourceTaskId,
				ExclusiveCategory = exclusiveCategory,
                Config = config,
                BinData = binData
            };

            _dbContext.Tasks.Add(task);

            _dbContext.SaveChanges();

            return task.ID;
        }

        /// <summary>
        /// получение id задачи на исполнение, то что одним запросом получается id ожидающей выполнения и апдейтится как выполняющая гарантирует
        ///что не будет конфликтов если во много потоков будет выполняться или вообще с разных машин в кластере
        /// </summary>
        /// <returns></returns>
        public int? TakeNewTaskForProcessing()
        {
			return _dbContext.Database.SqlQuery<int?>(@"UPDATE Tasks
														SET    StateID                  = @RunningStateId
														,      LastStatusChangeTime     = GETDATE()
														,      Progress                 = 0
															   OUTPUT INSERTED.ID
														WHERE  ID                       = (
																   SELECT TOP 1 ID
																   FROM   Tasks
																   WITH (ROWLOCK UPDLOCK)
																   WHERE  StateID       = @NewStateId AND (
																			  ExclusiveCategory IS NULL OR NOT EXISTS (
																				  SELECT *
																				  FROM   Tasks t
																				  WHERE  t.ExclusiveCategory = Tasks.ExclusiveCategory AND t.StateID = @RunningStateId
																			  )
																		  )
																   ORDER BY ID
															   )",
                    new SqlParameter("@NewStateId", State.New), new SqlParameter("@RunningStateId", State.Running))
                    .SingleOrDefault();
        }
        
        /// <summary>
        /// Получить задачу по ИД
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Null, если задача не найдена</returns>
        public Task GetTask(int id)
        {
            return _dbContext.Tasks.SingleOrDefault(x => x.ID == id);
        }


        public TaskModel GetTask(int id, Expression<Func<Task, TaskModel>> selector)
        {
            var query = _dbContext.Tasks.Where(x => x.ID == id).Select(selector);
            return query.FirstOrDefault();
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
                sql += string.Format(" AND StateID IN ({0})", string.Join(",", allowedInitialStates.Select(x=>(byte)x)));

			if (message != null && message.Length > TaskRunnerMaxMessageLength && _useTaskRunnerMessageLengthRestriction)
                message = message.Substring(0, TaskRunnerMaxMessageLength - 3) + "...";

            int rowsAffected = _dbContext.Database.ExecuteSqlCommand(sql,
							new SqlParameter("@StateId", state), 
                            new SqlParameter("@ID", id), 
                            new SqlParameter("@Message", (object)message ?? DBNull.Value), 
                            new SqlParameter("@Progress", (object)progress ?? DBNull.Value));

            return rowsAffected == 1;
        }

		

        public void Dispose()
        {
            if (_dbContext != null)
                _dbContext.Dispose();
        }

      

        public Task GetLastTask(int? userId, State? state = null, string key = null)
        {
            return
                _dbContext.Tasks.Include("TaskState")
                    .OrderByDescending(x => x.ID)
                    .FirstOrDefault(
                        x =>(x.UserID == userId || !userId.HasValue)
							&& (x.StateID == (byte?)state || !state.HasValue)
                            && (x.Name == key || key == null));
        }

        public bool Cancel(int id)
        {
            //если еще не начали исполнять то можно сразу в отмененную
			bool cancelledWhileNew = ChangeTaskState(id, State.Cancelled, string.Empty, new[] { State.New });
            
            if (cancelledWhileNew)
                return true;

            //если уже выполняется то максимум можно запросить отмену
            return RequestCancelation(id);
        }

        public bool Rerun(int id)
        {
			return ChangeTaskState(id, State.New, string.Empty, new[] { State.Cancelled, State.Done, State.Failed });
        }

	    public void SpawnTask(int sourceTaskId)
	    {
		    var sourceTask = GetTask(sourceTaskId);

		    if (sourceTask == null)
			    throw new ArgumentException("Task id=" + sourceTaskId + " not found");

		    AddTask(sourceTask.Name, sourceTask.Data, sourceTask.UserID, sourceTask.UserName, sourceTask.DisplayName, sourceTaskId);
	    }

	    public Task[] GetScheduledTasks()
	    {
			return _dbContext.Tasks.Include("Schedule").Where(x => x.Schedule.Enabled).ToArray();
	    }

	    public void SaveSchedule(int taskId, bool enabled, string cronExpression)
	    {
		    var task = GetTask(taskId);

			if (task == null)
				throw new ArgumentException("Task id=" + taskId + " not found");

		    if (string.IsNullOrEmpty(cronExpression))
		    {
			    if (task.Schedule != null)
			    {
				    _dbContext.Schedules.Remove(task.Schedule);

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

		    _dbContext.SaveChanges();
	    }


	    public Dictionary<int,string> GetAllStates()
	    {
		    return _dbContext.TaskStates.ToDictionary(x => x.ID, x => x.Name);
	    }

		public Task[] GetTasks(int skip, int take, int? userIdToFilterBy, int? stateIdToFilterBy, string nameFillter, bool? hasSchedule, out int totalCount)
	    {
			var tasksFiltered = _dbContext.Tasks.Include("TaskState").Include("Schedule")
			    .Where(
				    x =>
					    (!userIdToFilterBy.HasValue || x.UserID == userIdToFilterBy) &&
					    (!stateIdToFilterBy.HasValue || x.StateID == stateIdToFilterBy) &&
					    (nameFillter == null || x.DisplayName.Contains(nameFillter)) &&
						(!hasSchedule.HasValue || x.Schedule.Enabled == hasSchedule || !hasSchedule.Value && !x.ScheduleID.HasValue ));

		    totalCount = tasksFiltered.Count();

			return tasksFiltered
				.OrderByDescending(x => x.ID)
				.Skip(skip)
				.Take(take)
				.ToArray();
	    }


	    public bool RequestCancelation(int id)
        {
            int rowsAffected = _dbContext.Database.ExecuteSqlCommand(
             "UPDATE Tasks SET IsCancellationRequested = 1 WHERE ID=@ID AND StateID=@RunningStateId",
			 new SqlParameter("@RunningStateId", State.Running),
             new SqlParameter("@ID", id));

            return rowsAffected == 1;
        }

	    public void SetTaskProgress(int id, byte progress)
        {
            _dbContext.Database.ExecuteSqlCommand(
                            "UPDATE Tasks SET Progress=@Progress WHERE ID=@ID",
                            new SqlParameter("@Progress", progress), new SqlParameter("@ID", id));
        }

        public bool GetIsCancellationRequested(int taskId)
        {
            return
                _dbContext.Database.SqlQuery<bool>(
                    "SELECT IsCancellationRequested FROM Tasks WITH (nolock) WHERE ID=@ID",
                    new SqlParameter("@ID", taskId))
                    .SingleOrDefault();
        }
       
    }
}
