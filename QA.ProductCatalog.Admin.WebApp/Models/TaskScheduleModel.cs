using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using QA.Core.DPC.Resources;

namespace QA.ProductCatalog.Admin.WebApp.Models
{
    public class TaskScheduleModel
	{
        [Display(Name="TaskId", ResourceType = typeof(ControlStrings))]
		public int TaskId { get; set; }

        [Display(Name="Enabled", ResourceType = typeof(ControlStrings))]
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