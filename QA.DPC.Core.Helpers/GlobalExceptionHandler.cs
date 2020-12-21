using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using QA.Core.DPC.QP.Exceptions;

namespace QA.DPC.Core.Helpers
{
    public class GlobalExceptionHandler
    {
        private readonly ILoggerFactory _factory;
        private readonly Dictionary<Type, string> _redirectMap;

        public GlobalExceptionHandler(ILoggerFactory factory, Dictionary<Type, string> redirectMap = null)
        {
            _factory = factory;
            _redirectMap = redirectMap;
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
                    var logger = _factory.CreateLogger("Global Exception Handling");
                    LoggerExtensions.LogError(logger, new EventId(1), ex.Error, "Unhandled exception occurs");

                    if (_redirectMap.TryGetValue(ex.Error.GetType(), out string redirect))
                    {
                        context.Response.Redirect(redirect);
                    }
                    else
                    {
                        
                        var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                        await context.Response.WriteAsync(err).ConfigureAwait(false);
                    }
                }
            });
        }
    }
}