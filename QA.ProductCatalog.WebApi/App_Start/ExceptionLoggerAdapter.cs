using System;
using System.Net.Http;
using System.Text;
using System.Web.Http.ExceptionHandling;
using QA.Core;
using QA.Core.Logger;

namespace QA.ProductCatalog.WebApi.App_Start
{
    //из https://github.com/filipw/apress-recipes-webapi/blob/master/Chapter%2007/7-3/Apress.Recipes.WebApi/Apress.Recipes.WebApi/NLogExceptionLogger.cs
    //с правками
    public class ExceptionLoggerAdapter : ExceptionLogger
	{
		private static readonly Lazy<ILogger> LazyLogger = new Lazy<ILogger>(ObjectFactoryBase.Resolve<ILogger>);
		
		public override void Log(ExceptionLoggerContext context)
		{
			LazyLogger.Value.ErrorException(RequestToString(context.Request), context.Exception);
		}

		private static string RequestToString(HttpRequestMessage request)
		{
			var message = new StringBuilder();
			
			if (request.Method != null)
				message.Append(request.Method);

			if (request.RequestUri != null)
				message.Append(" ").Append(request.RequestUri);

			return message.ToString();
		}
	}
}