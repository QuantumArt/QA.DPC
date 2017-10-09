using System.Threading;
using System.Threading.Tasks;
using QA.Core;
using QA.Core.Logger;

namespace QA.Scheduler.API.Services
{
	public abstract class ProcessorBase : IProcessor
	{
		protected ILogger Logger { get; private set; }

		public ProcessorBase(ILogger logger)
		{
			Logger = logger;
		}

		public async Task Run(CancellationToken token)
		{
			Logger.LogDebug(() => GetType().Name + " start _______________________________________________________________________________________");
			await RunProcessor(token);
		}

		protected abstract Task RunProcessor(CancellationToken token);
	}
}
