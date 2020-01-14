using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public abstract class ActionTaskBase : IAction, ITask
	{
		public const string ResourceClass = "TaskStrings";
		protected ITaskExecutionContext TaskContext { get; private set; }
		protected ActionData ActionData { get; private set; }

		public ActionTaskBase()
		{
			TaskContext = new EmptyTaskExecutionContext();
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
				processResult = Process(context) ?? new ActionTaskResult();
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
						ResourceClass = ResourceClass, 
						ResourceName = "ArticlesProcessed", 
						Parameters = new object[] {idsStr}
					});
				}

				if (errors != null)
				{
					foreach (var err in errors)
					{
						processResult.Messages.Add(new ActionTaskResultMessage() {
							ResourceClass = ResourceClass, 
							ResourceName = err.Key, 
							Parameters = new object[] {String.Join(",", err)}
						});
					}
				}
			}

			executionContext.Result = processResult;
			
		}
		#endregion


        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            Run(data, executionContext);
        }
    }
}
