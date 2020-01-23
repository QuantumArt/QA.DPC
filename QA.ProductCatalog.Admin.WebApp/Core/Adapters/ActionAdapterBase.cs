using System;
using System.Linq;
using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;

namespace QA.ProductCatalog.Admin.WebApp.Core.Adapters
{
	public abstract class ActionAdapterBase : IAction
	{
		#region Fields and properties
		private readonly Func<string, ITask> _getTask;
		private readonly ITaskService _taskService;
		private readonly IUserProvider _userProvider;
		private readonly CustomActionService _customActionService;
		public string TaskKey { get; set; }
		#endregion

		#region Constructor
		public ActionAdapterBase(Func<string, ITask> getTask, ITaskService taskService, IUserProvider userProvider, CustomActionService customActionService)
		{
			_getTask = getTask;
			_taskService = taskService;
			_userProvider = userProvider;
			_customActionService = customActionService;
		}
		#endregion

		#region IAction implementation
		public virtual ActionTaskResult Process(ActionContext context)
		{
			if (ProcessInstantly(context))
			{
				return ProcessTask(context);
			}
			else
			{
				RegisterTask(context);
                return ActionTaskResult.Success(new ActionTaskResultMessage()
                {
                    ResourceClass = nameof(TaskStrings),
                    ResourceName = nameof(TaskStrings.ActionEnqueued),
                });
			}
		}

		#endregion

		#region Protected methods
		protected ActionTaskResult ProcessTask(ActionContext context)
		{
			var task = _getTask(TaskKey);
			var taskContext = new EmptyTaskExecutionContext();
			var actionData = new ActionData { ActionContext = context	};
			
            task.Run(ActionData.Serialize(actionData), null, null, taskContext);
			return taskContext.Result;
        }

		protected void RegisterTask(ActionContext context)
		{
			try
			{
				var customAction = _customActionService.ReadByCode(context.ActionCode);

				var dataForQueue = new ActionData
				{
					ActionContext = context,
					Description = customAction.Description,
					IconUrl = customAction.IconUrl
				};

				_taskService.AddTask(TaskKey, ActionData.Serialize(dataForQueue), _userProvider.GetUserId(), _userProvider.GetUserName(), customAction.Name);
			}
			catch (Exception ex)
			{
				throw new ActionException(TaskStrings.ActionErrorMessage, context.ContentItemIds.Select(id => new ProductException(id, TaskStrings.ErrorActionEnqueue, ex)), context);
			}
		}

		protected abstract bool ProcessInstantly(ActionContext context);
		#endregion
	}
}
