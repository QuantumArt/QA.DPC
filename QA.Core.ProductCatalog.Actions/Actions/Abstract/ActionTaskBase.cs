using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.Core.ProductCatalog.Actions.Actions.Abstract
{
	public abstract class ActionTaskBase : IAction, ITask
	{
		private const string StandartMessageTemplate = "Обработаны статьи: {0}";

		protected ITaskExecutionContext TaskContext { get; private set; }
		protected ActionData ActionData { get; private set; }

		public ActionTaskBase()
		{
			TaskContext = new EmptyTaskExecutionContext();
		}

		#region IAction implementation
		public abstract string Process(ActionContext context);
		#endregion

		#region ITask implementation
		public void Run(string data, ITaskExecutionContext executionContext)
		{
			TaskContext = executionContext;
			ActionData = ActionData.Deserialize(data);

			var context = ActionData.ActionContext;
			string messageFromProcess = null;
			IEnumerable<IGrouping<string, int>> errors = null;
			var ids = context.ContentItemIds;

			try
			{
				messageFromProcess = Process(context);
			}
			catch (ActionException ex)
			{
				var failedIds = ex.InnerExceptions.OfType<ProductException>().Select(x => x.ProductId);
				ids = ids.Except(failedIds).ToArray();

				errors = ex.InnerExceptions
					.OfType<ProductException>()
					.GroupBy(
						e => e.InnerException == null ? e.Message : e.Message + " : " + e.InnerException.Message,
						e => e.ProductId
					);
			}

			if (executionContext.IsCancelled)
			{
				executionContext.Message = string.Empty;
			}
			else if (string.IsNullOrEmpty(messageFromProcess))
			{
				var sb = new StringBuilder();

				if (ids.Any())
				{
					sb.AppendFormat(StandartMessageTemplate, string.Join(", ", ids));
				}

				if (errors != null)
				{
					foreach (var err in errors)
					{
						sb.AppendLine();
						sb.AppendFormat("{0} : статьи {1}", err.Key, string.Join(", ", err));
					}
				}
				
				executionContext.Message = sb.ToString();
			}
			else
			{
				executionContext.Message = messageFromProcess;
			}
			
			if (!ids.Any() && errors != null)
			{
				throw new ActionException(executionContext.Message, new Exception[0], null);
			}
		}
		#endregion


        public void Run(string data, string config, byte[] binData, ITaskExecutionContext executionContext)
        {
            Run(data, executionContext);
        }
    }
}
