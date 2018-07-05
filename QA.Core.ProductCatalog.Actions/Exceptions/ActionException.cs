using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace QA.Core.ProductCatalog.Actions.Exceptions
{
    [Serializable]
	public class ActionException : AggregateException
	{
		private const string ContextKey = "Context";

		public ActionContext Context { get; private set; }

		public ActionException(string message, IEnumerable<Exception> innerExceptions, ActionContext context)
			: base(message, innerExceptions)
		{
			Context = context;
		}

		protected ActionException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			Context = (ActionContext)info.GetValue(ContextKey, typeof(ActionContext));
		}

		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(ContextKey, Context, typeof(ActionContext));
			base.GetObjectData(info, context);
		}
	}
}