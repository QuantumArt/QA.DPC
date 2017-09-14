using System.Web.Http.Filters;
using QA.Core;
using QA.Core.Logger;

namespace QA.ProductCatalog.FileSyncWebHost.Filters
{
	public class ExceptionLoggerFilterAttribute : ExceptionFilterAttribute
	{
		private readonly ILogger _loggeer;

		public ExceptionLoggerFilterAttribute(ILogger loggeer)
		{
			_loggeer = loggeer;
		}

		public override void OnException(HttpActionExecutedContext context)
		{
			var description = new
			{
				controller = context.ActionContext.ControllerContext.ControllerDescriptor.ControllerName,
				action = context.ActionContext.ActionDescriptor.ActionName
			};

			_loggeer.ErrorException(description.ToString(), context.Exception);
		}
	}
}