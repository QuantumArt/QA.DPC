using Microsoft.AspNetCore.Mvc.Filters;
using QA.Core.Logger;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class ExceptionLoggerFilterAttribute : ExceptionFilterAttribute
	{
		private readonly ILogger _logger;

		public ExceptionLoggerFilterAttribute(ILogger logger)
		{
			_logger = logger;
		}

		public override void OnException(ExceptionContext context)
		{
			var description = new
			{
				controller = context.RouteData.Values["controller"],
				action = context.RouteData.Values["action"]
			};

			_logger.ErrorException(description.ToString(), context.Exception);
		}
	}
}