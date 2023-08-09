using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class TaskModel
    {
        public int Id { get; set; }

        [Display(Name="CreatedDate", ResourceType = typeof(ControlStrings))]
        public DateTime CreatedTime { get; set; }

        [Display(Name="LastStatusChangeTime", ResourceType = typeof(ControlStrings))]
        public DateTime? LastStatusChangeTime { get; set; }

        [Display(Name="Key", ResourceType = typeof(ControlStrings))]
        public string Name { get; set; }

        [Display(Name="DisplayName", ResourceType = typeof(ControlStrings))]
        public string DisplayName { get; set; }

        [Display(Name="State", ResourceType = typeof(ControlStrings))]
        public string State { get; set; }

        [Display(Name="UserName", ResourceType = typeof(ControlStrings))]
        public string UserName { get; set; }

        [Display(Name="Message", ResourceType = typeof(ControlStrings))]
        public string Message { get; set; }

        [Display(Name="Progress", ResourceType = typeof(ControlStrings))]
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
        public bool ScheduleEnabled { get; set; }
	    public string ScheduleCronExpression { get; set; }
        public bool AllowConcurrentTasks { get; set; }

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

	        HasSchedule = efTask.ScheduleID.HasValue;
            
            ScheduleEnabled = HasSchedule && (efTask.Schedule?.Enabled ?? false);

            AllowConcurrentTasks = efTask.Schedule?.AllowConcurrentTasks ?? true;

            ScheduleCronExpression = HasSchedule ? efTask.Schedule?.CronExpression : null;
        }
    }
}