using System;
using System.Collections.Generic;
using System.Diagnostics;
using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.Core.ProductCatalog.Actions.Exceptions;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using Quantumart.QP8.BLL.Services.API;

namespace QA.ProductCatalog.Admin.WebApp.Core.Adapters
{
	public class TimebasedActionAdapter : ActionAdapterBase
	{
		private const string IntervalKey = "Interval";
		private const int DefaultInterval = 10000;

		#region Constructor
		public TimebasedActionAdapter(Func<string, ITask> getTask, ITaskService taskService, IUserProvider userProvider, CustomActionService customActionService)
			: base(getTask, taskService, userProvider, customActionService)
		{
		}
		#endregion

		public override ActionTaskResult Process(ActionContext context)
		{
			var ids = new Queue<int>(context.ContentItemIds);
			var sw = new Stopwatch();
			var interval = GetInterval(context);
			var taskIds = new List<int>();

			sw.Start();

			while (ids.Count > 0 && sw.ElapsedMilliseconds <= interval)
			{
				int id = ids.Dequeue();
				context.ContentItemIds = new[] { id };
				try
				{
					ProcessTask(context);
				}
				catch (ProductException)
				{
					taskIds.Add(id);
				}
			}

			sw.Stop();
			taskIds.AddRange(ids);

			if (taskIds.Count > 0)
			{
				context.ContentItemIds = taskIds.ToArray();
				RegisterTask(context);
				return ActionTaskResult.Success(TaskStrings.ActionEnqueued);
			}
			else
			{
				return null;
			}
		}

		protected override bool ProcessInstantly(ActionContext context)
		{
			throw new NotImplementedException();
		}

		private int GetInterval(ActionContext context)
		{
			int intterval = DefaultInterval;

			if (context.Parameters.ContainsKey(IntervalKey))
			{
				if (!int.TryParse(context.Parameters[IntervalKey], out intterval))
				{
					intterval = DefaultInterval;
				}
			}

			return intterval;
		}
	}
}
