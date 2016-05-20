using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;

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

		public string Process(ActionContext context)
		{
			var token = CallMethod(
				"Process", "ContentItemIds = {0}, ContentId = {1}, CustomerCode = {2}, BackendSid = {3}",
				string.Join(",", context.ContentItemIds),
				context.ContentId,
				context.CustomerCode,
				context.BackendSid);

			string message = _action.Process(context);

			EndMethod(token);
			return message;
		}
	}
}
