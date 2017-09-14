using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QA.Core;
using QA.Core.Logger;
using QA.Scheduler.API.Services;

namespace QA.Scheduler.Core
{
	internal sealed class Scheduler : IScheduler, IDisposable
	{
		private const string ServiceRepeatIntervalKey = "ServiceRepeatInterval";
		private const string ServiceRepeatOnErrorIntervalKey = "ServiceRepeatOnErrorInterval";

		private readonly IEnumerable<IProcessor> _processors;
		private readonly ILogger _logger;
		private readonly CancellationTokenSource _cts;
		private readonly TimeSpan _repeatInterval;
		private readonly TimeSpan _repeatOnErrorInterval;
		private Task[] tasks { get; set; }

		public Scheduler(IEnumerable<IProcessor> processors, ILogger logger)
		{
			if (!TimeSpan.TryParse(ConfigurationManager.AppSettings[ServiceRepeatIntervalKey], out _repeatInterval))
			{
				_repeatInterval = TimeSpan.FromSeconds(10);
			}

			if (!TimeSpan.TryParse(ConfigurationManager.AppSettings[ServiceRepeatOnErrorIntervalKey], out _repeatOnErrorInterval))
			{
				_repeatOnErrorInterval = TimeSpan.FromMinutes(10);
			}

			_processors = processors;
			_logger = logger;
			_cts = new CancellationTokenSource();
		}

		public void Start()
		{
			_logger.Info("Service is started {0}", new { RepeatInterval = _repeatInterval, RepeatOnErrorInterval = _repeatOnErrorInterval });
			tasks = RunProcessors(_cts.Token).ToArray();
		}

		public void Stop()
		{
			_logger.Info("Service is stoping...");
			_cts.Cancel();

			try
			{
				Task.WaitAll(tasks);
			}
			catch (AggregateException)
			{
			}

			_logger.Info("Service is stoped");
		}

		public void Dispose()
		{
			_cts.Dispose();
		}

		private IEnumerable<Task> RunProcessors(CancellationToken cancellationToken)
		{
			foreach (var p in _processors)
			{
				var processor = p;

				yield return Task.Run(async () =>
				{
					TimeSpan interval;

					while (!cancellationToken.IsCancellationRequested)
					{
						try
						{
							await processor.Run(cancellationToken);
							interval = _repeatInterval;
						}
						catch (OperationCanceledException)
						{
							throw;
						}
						catch (Exception ex)
						{
							_logger.ErrorException("Service can't run " + processor.GetType().Name, ex);
							interval = _repeatOnErrorInterval;
						}

						await Task.Delay(interval, cancellationToken);
					}
				}, cancellationToken);
			}
		}
	}
}
