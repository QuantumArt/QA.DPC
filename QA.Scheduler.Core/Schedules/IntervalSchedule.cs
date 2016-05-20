using System;
using QA.Scheduler.API.Models;
using QA.Scheduler.API.Services;

namespace QA.Scheduler.Core.Schedules
{
	public class IntervalSchedule : ISchedule
	{
		private readonly TimeSpan _interval;

		public IntervalSchedule(TimeSpan interval)
		{
			_interval = interval;
		}

		public bool NeedProcess(SchedulerContext context)
		{
			return context.CurrentTime - context.LastEndTime > _interval;
		}
	}
}
