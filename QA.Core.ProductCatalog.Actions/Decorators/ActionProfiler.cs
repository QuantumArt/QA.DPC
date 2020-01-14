using System;
using QA.Core.Logger;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
    public class ActionProfiler : ProfilerBase, IAction
	{
		private readonly IAction _action;

		public ActionProfiler(IAction action, ILogger logger)
			: base(logger)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			_action = action;
			Service = _action.GetType().Name;
		} 

		public ActionTaskResult Process(ActionContext context)
		{
			var token = CallMethod(
				"Process", "ContentItemIds = {0}, ContentId = {1}, CustomerCode = {2}, BackendSid = {3}",
				string.Join(",", context.ContentItemIds),
				context.ContentId,
				context.CustomerCode,
				context.BackendSid);

			var message = _action.Process(context);

			EndMethod(token);
			return message;
		}
	}
}
