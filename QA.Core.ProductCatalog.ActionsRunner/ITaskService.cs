using System;
using System.Collections.Generic;
using QA.Core.ProductCatalog.ActionsRunnerModel;

namespace QA.Core.ProductCatalog.ActionsRunner
{
    public interface ITaskService : IDisposable
    {
		int AddTask(string key, string data, int userId, string userName, string taskDisplayName, int? sourceTaskId = null, string exclusiveCategory=null, string config=null, byte[] binData = null);
        void SetTaskProgress(int taskId, byte progress);
        bool GetIsCancellationRequested(int taskId);

        int? TakeNewTaskForProcessing();

        Task GetTask(int taskId, bool convertMessage = false);

		bool ChangeTaskState(int id, State state, string message, State[] allowedInitialStates = null);
		Task GetLastTask(int? userId, State? state = null, string key = null);

        bool Cancel(int taskId);

        bool Rerun(int taskId);

		void SpawnTask(int sourceTaskId);

		Task[] GetScheduledTasks();

		void SaveSchedule(int taskId, bool enabled, string cronExpression);

		void CancelRequestedTasks();

		Dictionary<int, string> GetAllStates();

		Task[] GetTasks(int skip, int take, int? userIdToFilterBy, int? stateIdToFilterBy, string nameFilter, bool? hasSchedule, DateTime? createdLower, DateTime? createdUpper, out int totalCount);
	}
}
