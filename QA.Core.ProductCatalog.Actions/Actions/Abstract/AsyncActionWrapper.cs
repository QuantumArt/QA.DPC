using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NLog;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
    public abstract class AsyncActionWrapper : IAction, IAsyncAction
    {
        protected readonly NLog.Logger Logger;

        public AsyncActionWrapper()
        {
            Logger = LogManager.GetLogger(GetType().ToString());
        }

        public abstract Task<ActionTaskResult> Process(ActionContext context);

        ActionTaskResult IAction.Process(ActionContext context)
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
                Logger.Info($"{transactionId} {caller} {message} Elapsed {timer.ElapsedMilliseconds}");
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
                Logger.Info($"{transactionId} {caller} {message} Elapsed {timer.ElapsedMilliseconds}");
            }
        }
	}
}
