using System;
using System.Web.Http.ExceptionHandling;
using QA.Core;

namespace QA.ProductCatalog.HighloadFront.Filters
{
    public class GlobalExceptionLogger : ExceptionLogger
    {
        private readonly ILogger _logger;

        public GlobalExceptionLogger(ILogger logger)
        {
            _logger = logger;
        }

        public override void Log(ExceptionLoggerContext context)
        {
            string message = null;

            if (context.Request != null)
                message = $"Error processing {context.Request.Method} request to {context.Request.RequestUri}" + Environment.NewLine;

            message += context.Exception.ToString();

            _logger.Error(message);

            base.Log(context);
        }
    }
}