using System;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;

namespace QA.ProductCatalog.Admin.WebApp.Core.Adapters
{
	public class CountActionAdapter : ActionAdapterBase
	{
		private const string CountKey = "Count";
		private const int DefaultCount = 1;

		#region Constructor
		public CountActionAdapter(Func<string, ITask> getTask, ITaskService taskService, IUserProvider userProvider, CustomActionService customActionService)
			: base(getTask, taskService, userProvider, customActionService)
		{
		}
		#endregion

		protected override bool ProcessInstantly(ActionContext context)
		{
			int count = DefaultCount;

			if (context.Parameters.ContainsKey(CountKey))
			{
				if (!int.TryParse(context.Parameters[CountKey], out count))
				{
					count = DefaultCount;
				}
			}

			return context.ContentItemIds.Length <= count;
		}
	}
}
