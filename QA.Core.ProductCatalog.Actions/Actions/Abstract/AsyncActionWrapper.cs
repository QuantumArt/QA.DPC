using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
    public abstract class AsyncActionWrapper : IAction, IAsyncAction
    {
        protected readonly ILogger Logger;

        public AsyncActionWrapper(ILogger logger)
        {
            Logger = logger;
        }

        public abstract Task<string> Process(ActionContext context);

		string IAction.Process(ActionContext context)
		{
			var task = Process(context);
			Task.WaitAll(task);
			return task.Result;
		}

        protected T DoWithLogging<T>(string message, string transactionId, Func<T> func, [CallerMemberName] string caller = "")
        {
            var timer = new Stopwatch();

            try
            {
                T result;
                timer.Start();
                result = func();
                return result;
            }
            finally
            {
                timer.Stop();
                Logger.Info(string.Format("{0} {1} {2} Elapsed {3}", transactionId, caller, message, timer.ElapsedMilliseconds));
            }
        }

        protected void DoWithLogging(string message, string transactionId, Action func, [CallerMemberName] string caller = "")
        {
            var timer = new Stopwatch();
            timer.Start();

            try
            {
                func();
            }
            finally
            {
                timer.Stop();
                Logger.Info(string.Format("{0} {1} {2} Elapsed {3}", transactionId, caller, message, timer.ElapsedMilliseconds));
            }
        }
	}
}
