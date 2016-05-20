using QA.Scheduler.API.Models;

namespace QA.Scheduler.API.Services
{
	public interface ISchedule
	{
		bool NeedProcess(SchedulerContext context);
	}
}
