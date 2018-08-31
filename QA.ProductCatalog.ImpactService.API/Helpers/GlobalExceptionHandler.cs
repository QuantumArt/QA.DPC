using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace QA.ProductCatalog.ImpactService.API.Helpers
{
    public class GlobalExceptionHandler
    {
        private readonly ILogger _logger;
        public GlobalExceptionHandler(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("Global Exception Handling");
        }

        public void Action(IApplicationBuilder options)
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/html";
                var ex = context.Features.Get<IExceptionHandlerFeature>();
                if (ex != null)
                {
                    _logger.LogError(new EventId(1), ex.Error, "Unhandled exception occurs");
                    var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                    await context.Response.WriteAsync(err).ConfigureAwait(false);
                }
            });
        }
    }
}