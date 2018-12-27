using System;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;

namespace QA.ProductCatalog.Admin.WebApp.Core.Adapters
{
	public class TaskActionAdapter : ActionAdapterBase
	{
		#region Constructor
		public TaskActionAdapter(Func<string, ITask> getTask, ITaskService taskService, IUserProvider userProvider, CustomActionService customActionService)
			: base(getTask, taskService, userProvider, customActionService)
		{
		}
		#endregion

		protected override bool ProcessInstantly(ActionContext context)
		{
			return false;
		}
	}
}