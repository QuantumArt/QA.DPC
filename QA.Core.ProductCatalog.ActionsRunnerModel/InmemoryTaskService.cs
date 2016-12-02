using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using QA.Core.ProductCatalog.ActionsRunnerModel.EntityModels;
using System.Collections.Concurrent;
using System.Threading;

namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    /// <summary>
    /// Добавление задач thread safe. выполнение - нет. одновременно должен быть ровно один runner
    /// </summary>
    public class InmemoryTaskService : ITaskService
    {
        private  ConcurrentDictionary<int,Task> _tasks = new ConcurrentDictionary<int, Task>();

        private int _taskNumber = 0;

        public int AddTask(string key, string data, int userId, string userName, string taskDisplayName, int? sourceTaskId = default(int?), string exclusiveCategory = null, string config = null, byte[] binData = null)
        {
            int newTaskId = Interlocked.Increment(ref _taskNumber);

            _tasks.TryAdd(newTaskId, new Task
            {
                ID= newTaskId,
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
            });

            return newTaskId;
        }

        public bool Cancel(int taskId)
        {
            throw new NotImplementedException();
        }

        public bool ChangeTaskState(int id, State state, string message, State[] allowedInitialStates = null)
        {
            var task = GetTask(id);

            if (allowedInitialStates != null && allowedInitialStates.Length > 0 && !allowedInitialStates.Contains((State)task.StateID))
                return false;

            task.StateID = (int)state;

            task.Message = message;

            if (task.StateID == (int)State.Done && task.Progress != null && task.Progress != 100)
                task.Progress = 100;

            return true;
        }

        public void Dispose()
        {
        }

        public Dictionary<int, string> GetAllStates()
        {
            return Enum.GetValues(typeof(State)).Cast<State>().ToDictionary(x => (int)x, x => x.ToString());
        }

        public bool GetIsCancellationRequested(int taskId)
        {
            return GetTask(taskId).IsCancellationRequested;
        }

        public Task GetLastTask(int? userId, State? state = default(State?), string key = null)
        {
            throw new NotImplementedException();
        }

        public Task[] GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        public Task GetTask(int taskId)
        {
            return _tasks[taskId];
        }

        public TaskModel GetTask(int id, Expression<Func<Task, TaskModel>> selector)
        {
            throw new NotImplementedException();
        }

        public Task[] GetTasks(int skip, int take, int? userIdToFilterBy, int? stateIdToFilterBy, string nameFillter, bool? hasSchedule, out int totalCount)
        {
            totalCount = _tasks.Count();
            return _tasks.Values.Skip(skip).Take(take).ToArray();
        }

        public bool Rerun(int taskId)
        {
            throw new NotImplementedException();
        }

        public void SaveSchedule(int taskId, bool enabled, string cronExpression)
        {
            throw new NotImplementedException();
        }

        public void SetTaskProgress(int taskId, byte progress)
        {
            GetTask(taskId).Progress = progress;
        }

        public void SpawnTask(int sourceTaskId)
        {
            throw new NotImplementedException();
        }

        public int? TakeNewTaskForProcessing()
        {
            var newTask = _tasks.Where(x => x.Value.StateID == (byte)State.New).Select(x=>x.Value).FirstOrDefault();

            if (newTask == null)
                return null;

            newTask.StateID = (byte)State.Running;

            return newTask.ID;
        }
    }
}
