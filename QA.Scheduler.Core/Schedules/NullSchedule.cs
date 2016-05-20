using QA.Scheduler.API.Models;
using QA.Scheduler.API.Services;

namespace QA.Scheduler.Core.Schedules
{
	public class NullSchedule : ISchedule
	{
		public bool NeedProcess(SchedulerContext context)
		{
			return true;
		}
	}
}
