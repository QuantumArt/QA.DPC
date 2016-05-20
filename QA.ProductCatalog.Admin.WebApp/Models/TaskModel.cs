using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using QA.Core.ProductCatalog.ActionsRunnerModel;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [DisplayName("Дата создания")]
        public DateTime CreatedTime { get; set; }

        [DisplayName("Последнаяя смена статуса")]
        public DateTime? LastStatusChangeTime { get; set; }

        [DisplayName("Ключ")]
        public string Name { get; set; }

        [DisplayName("Имя")]
        public string DisplayName { get; set; }

        [DisplayName("Статус")]
        public string State { get; set; }

        [DisplayName("Заказчик")]
        public string UserName { get; set; }

        [DisplayName("Сообщение")]
        public string Message { get; set; }

        [DisplayName("Процент выполнения")]
        public byte? Progress { get; set; }

        public string IconName
        {
			get { return StateIcons.ContainsKey((State)StateId) ? StateIcons[(State)StateId] : null; }
        }

		public bool IsCancellationRequested { get; set; }
		public int? ScheduledFromTaskId { get; set; }

		private static readonly Dictionary<State, string> StateIcons = new Dictionary<State, string>
        {
            {QA.Core.ProductCatalog.ActionsRunnerModel.State.New, "New"},
            {QA.Core.ProductCatalog.ActionsRunnerModel.State.Running, "Running"},
            {QA.Core.ProductCatalog.ActionsRunnerModel.State.Done, "Done"},
            {QA.Core.ProductCatalog.ActionsRunnerModel.State.Failed, "Failed"},
            {QA.Core.ProductCatalog.ActionsRunnerModel.State.Cancelled, "Cancelled"}
        };

        public int StateId { get; set; }
	    public bool HasSchedule { get; set; }
	    public string ScheduleCronExpression { get; set; }

	    public TaskModel()
        {
        }

        public TaskModel(Task efTask)
        {
            Id = efTask.ID;

            CreatedTime = efTask.CreatedTime;

            LastStatusChangeTime = efTask.LastStatusChangeTime;

            Name = efTask.Name;

            State = efTask.TaskState.Name;

            UserName = efTask.UserName;

            Progress = efTask.Progress;

            Message = efTask.Message;

            DisplayName = efTask.DisplayName;

            StateId = efTask.StateID;

            IsCancellationRequested = efTask.IsCancellationRequested;

			ScheduledFromTaskId = efTask.ScheduledFromTaskID;

	        HasSchedule = efTask.ScheduleID.HasValue && efTask.Schedule.Enabled;

	        ScheduleCronExpression = HasSchedule ? efTask.Schedule.CronExpression : null;
        }
    }
}