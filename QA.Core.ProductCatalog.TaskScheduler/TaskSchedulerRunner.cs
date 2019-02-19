using System;
using System.Linq;
using System.Threading;
using System.Timers;
using QA.Core.Logger;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;

namespace QA.Core.ProductCatalog.TaskScheduler
{
	public class TaskSchedulerRunner
	{
		private readonly ISchedulerFactory _schedulerFactory;
		private readonly Func<ITaskService> _taskServiceFunc;

		public TaskSchedulerRunner(ISchedulerFactory schedulerFactory, Func<ITaskService> taskServiceFunc, ILogger logger)
		{
			_schedulerFactory = schedulerFactory;

			_taskServiceFunc = taskServiceFunc;

			_logger = logger;
		}

		private int _secondsToUpdateJobsAndTriggers = 5;
		private IScheduler _scheduler;

		public int SecondsToUpdateJobsAndTriggers
		{
			set
			{
				if (value <= 0)
					throw new ArgumentException("value must be > 0");

				_secondsToUpdateJobsAndTriggers = value;
			}
			get { return _secondsToUpdateJobsAndTriggers; }
		}

		public void Stop()
		{
			lock (_stateLocker)
			{
				if (_scheduler == null)
					throw new Exception("Start must be called before Stop");

				_timerForSchedulerUpdate.Stop();

				_scheduler.Shutdown(true);
			}
		}

		private readonly object _stateLocker = new object();

		private void UpdateJobsAndTriggers()
		{
			using (var taskService = _taskServiceFunc())
			{
				_scheduler.JobFactory = new TaskServiceJobFactory(taskService);
				
				var tasks = taskService.GetScheduledTasks();

				foreach (var task in tasks)
				{
					var jobKey = GetJobKey(task);

					var existingJob = _scheduler.GetJobDetail(jobKey).Result;

					var triggerKey = GetTriggerKey(task);

					//добавили джоб
					if (existingJob == null)
					{
						ScheduleJobIfNotInPast(CreateJob(task), task);
					}
					else
					{
						var existingTriggers = _scheduler.GetTriggersOfJob(existingJob.Key).Result;

						//джоб уже есть но могли изменить триггер
						if (existingTriggers == null || !existingTriggers.Any(x => x.Key.Equals(triggerKey)))
						{
							if (existingTriggers != null)
								foreach (var existingTrigger in existingTriggers)
									_scheduler.UnscheduleJob(existingTrigger.Key);

							ScheduleJobIfNotInPast(existingJob, task);
						}
					}
				}


				var deletedTriggers =
					_scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.AnyGroup()).Result
						.Where(x => tasks.All(task => !GetTriggerKey(task).Equals(x)))
						.ToArray();

				foreach (var deletedTrigger in deletedTriggers)
					_scheduler.UnscheduleJob(deletedTrigger);
			}
		}

		private void ScheduleJobIfNotInPast(IJobDetail jobDetail, Task task)
		{
			var triggerForJob = CreateTrigger(task);

			var calendar = triggerForJob.CalendarName == null ? null : _scheduler.GetCalendar(triggerForJob.CalendarName).Result;

			DateTimeOffset? firstFireDateTimeOffset = ((IOperableTrigger)triggerForJob).ComputeFirstFireTimeUtc(calendar);

			//могут быть одноразовые расписания и если оно в прошлом то кварц будет кидать эксепшн при вызове ScheduleJob с такими
			//старые одноразовые удаляем
			if (firstFireDateTimeOffset.HasValue)
				_scheduler.ScheduleJob(jobDetail, triggerForJob);
			else
				using (var taskService = _taskServiceFunc())
				{
					taskService.SaveSchedule(task.ID, false, null);
				}
		}

		private static JobKey GetJobKey(Task task)
		{
			return new JobKey(task.ID.ToString());
		}

		private static TriggerKey GetTriggerKey(Task task)
		{
			return new TriggerKey(task.ID.ToString() + task.Schedule.CronExpression.GetHashCode());
		}

		private static IJobDetail CreateJob(Task task)
		{
			return JobBuilder.Create<TaskJob>().WithIdentity(GetJobKey(task))
				.UsingJobData("SourceTaskId", task.ID)
				.Build();
		}

		private static ITrigger CreateTrigger(Task task)
		{
			string[] components = task.Schedule.CronExpression.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

			if (components.Length < 5)
				throw new Exception("Некорректное выражение. Должно иметь не менее 5 компонентов");

			//из http://www.quartz-scheduler.net/documentation/quartz-2.x/tutorial/crontrigger.html
			//Support for specifying both a day-of-week and a day-of-month value is not complete (you must currently use the '?' character in one of these fields).
			if (components[2] != "?" && components[4] != "?")
			{
				if (components[2] == "*")
					components[2] = "?";
				else
					components[4] = "?";
			}

			//в бд лежит без секунд а кварц поддерживает с секундами только так что 0 прибавляем
			string cronExprFixed = "0 " + string.Join(" ", components);
			
			return TriggerBuilder.Create()
				.WithIdentity(GetTriggerKey(task))
				.WithSchedule(CronScheduleBuilder.CronSchedule(cronExprFixed))
				.Build();
		}

		private System.Timers.Timer _timerForSchedulerUpdate;
		private ILogger _logger;

		public void Start()
		{
			lock (_stateLocker)
			{
				if (_scheduler == null)
				{
					_scheduler = _schedulerFactory.GetScheduler().Result;
				}
				
				else if (_scheduler.IsStarted)
					throw new Exception("Scheduler already started");

				UpdateJobsAndTriggers();

				_scheduler.Start();

				if (_timerForSchedulerUpdate == null)
				{
					_timerForSchedulerUpdate = new System.Timers.Timer(SecondsToUpdateJobsAndTriggers * 1000) { AutoReset = true };

					_timerForSchedulerUpdate.Elapsed += _timerForSchedulerUpdate_Elapsed;
				}

				_timerForSchedulerUpdate.Start();
			}
		}

		void _timerForSchedulerUpdate_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (Monitor.TryEnter(_stateLocker))
			{
				try
				{
					UpdateJobsAndTriggers();
				}
				catch (Exception ex)
				{
					try
					{
						_logger.ErrorException("Ошибка при попытке загрузить актуальные триггеры и джобы из бд", ex);
					}
					catch
					{
					}
				}
				finally
				{
					Monitor.Exit(_stateLocker);
				}
			}

		}
	}

}
