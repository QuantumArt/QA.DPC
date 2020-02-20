using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Integration;
using Quantumart.QP8.BLL;


namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public abstract class ActionTaskBase : IAction, ITask
	{
		public const string ResourceClass = "TaskStrings";
		
		protected ILogger Logger { get; set; }
		protected ITaskExecutionContext TaskContext { get; private set; }
		protected ActionData ActionData { get; private set; }

		public ActionTaskBase()
		{
			TaskContext = new EmptyTaskExecutionContext();
			Logger = LogManager.GetLogger(GetType().ToString());
		}

		#region IAction implementation
		public abstract ActionTaskResult Process(ActionContext context);
		#endregion

		#region ITask implementation
		public void Run(string data, ITaskExecutionContext executionContext)
		{
			TaskContext = executionContext;
			ActionData = ActionData.Deserialize(data);

			var context = ActionData.ActionContext;
			ActionTaskResult processResult = new ActionTaskResult();
			IEnumerable<IGrouping<string, int>> errors = null;
			var ids = context.ContentItemIds;

			try
			{
				Logger.Info()
					.Message("{action} has been started", GetType().Name)
					.Property("taskId",TaskContext.TaskId)
					.Property("context", context)
					.Write();

				HttpContextUserProvider.ForcedUserId = context.UserId;
				processResult = Process(context) ?? new ActionTaskResult();
				HttpContextUserProvider.ForcedUserId = 0;
			}
			catch (ActionException ex)
			{
				var failedIds = ex.InnerExceptions.OfType<ProductException>().Select(x => x.ProductId);
				processResult.FailedIds = processResult.FailedIds.Union(failedIds).ToArray();

				errors = ex.InnerExceptions
					.OfType<ProductException>()
					.GroupBy(
						e => e.InnerException == null ? e.Message : e.Message + " : " + e.InnerException.Message,
						e => e.ProductId
					);
			}

			if (!executionContext.IsCancelled)
			{
				ids = ids.Except(processResult.FailedIds).ToArray();
				
				if (ids.Any())
				{
					processResult.IsSuccess = true;
					
					var idsStr = string.Join(", ", ids);
					processResult.Messages.Add(new ActionTaskResultMessage() {
						ResourceClass = nameof(TaskStrings), 
						ResourceName = nameof(TaskStrings.ArticlesProcessed), 
						Parameters = new object[] {idsStr}
					});
				}

				if (errors != null)
				{
					foreach (var err in errors)
					{
						var result = ActionTaskResult.FromString(err.Key);
						if (result != null)
						{
							if (result.Messages != null && result.Messages.Any())
							{
								processResult.Messages.AddRange(result.Messages);
							}
						}
						else
						{
							processResult.Messages.Add(new ActionTaskResultMessage() {
								ResourceClass = nameof(TaskStrings), 
								ResourceName = err.Key, 
								Parameters = new object[] {String.Join(",", err)}
							});		
						}
					}
				}
			}

			executionContext.Result = processResult.GetMergedResult();
			
		}
		
		#endregion
		

        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            Run(data, executionContext);
        }

        protected T DoWithLogging<T>(Func<T> func, string message, params object[] messageParams)
        {
	        var timer = new Stopwatch();

	        try
	        {
		        T result;
		        timer.Start();
		        result = func();
		        timer.Stop();
		        Logger.Info()
			        .Message(message, messageParams)
			        .Property("taskId", TaskContext.TaskId)
			        .Property("elapsed", timer.ElapsedMilliseconds)
			        .Write();                
		        return result;
	        }
	        catch (Exception ex)
	        {
		        timer.Stop();
		        Logger.Error()
			        .Exception(ex)
			        .Message(message, messageParams)
			        .Property("taskId", TaskContext.TaskId)
			        .Property("elapsed", timer.ElapsedMilliseconds)
			        .Write();

		        throw;
	        }
            

        }

        protected void DoWithLogging(Action func, string message, params object[] messageParams)
        {
	        var timer = new Stopwatch();
	        timer.Start();

	        try
	        {
		        func();
	        }
	        catch (Exception ex)
	        {
		        timer.Stop();
		        Logger.Error()
			        .Exception(ex)
			        .Message(message, messageParams)
			        .Property("taskId", TaskContext.TaskId)
			        .Property("elapsed", timer.ElapsedMilliseconds)
			        .Write();

		        throw;
	        }
	        
	        timer.Stop();
	        Logger.Info()
		        .Message(message, messageParams)
		        .Property("taskId", TaskContext.TaskId)
		        .Property("elapsed", timer.ElapsedMilliseconds)
		        .Write();        

        }
	}
}
