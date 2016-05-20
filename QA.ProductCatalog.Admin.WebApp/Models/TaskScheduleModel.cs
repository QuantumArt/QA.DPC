using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
	public class TaskScheduleModel
	{
		[DisplayName("ID задачи")]
		public int TaskId { get; set; }

		[DisplayName("Включено")]
		public bool Enabled { get; set; }


		public string CronExpression { get; set; }

		public TaskScheduleModel()
		{
		}

		public TaskScheduleModel(QA.Core.ProductCatalog.ActionsRunnerModel.Schedule efTaskSchedule)
		{
			Id = efTaskSchedule.ID;

			TaskId = efTaskSchedule.Tasks.Single().ID;

			Enabled = efTaskSchedule.Enabled;

			CronExpression = efTaskSchedule.CronExpression;
		}

		public int Id { get; set; }
	}
}