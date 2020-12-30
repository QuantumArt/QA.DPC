using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QA.Core.DPC.QP.Exceptions;
using Unity;

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

                    if (_redirectMap != null && _redirectMap.TryGetValue(ex.Error.GetType(), out string redirect))
                    {
                        var url = $"{context.Request.PathBase}{redirect}";
                        context.Response.Redirect(url);
                    }
                    else
                    {
                        
                        var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace}";
                        await context.Response.WriteAsync(err).ConfigureAwait(false);
                    }
                }
            });
        }

        public void ApiAction(IApplicationBuilder options)
        {
            options.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var ex = context.Features.Get<IExceptionHandlerFeature>();
                if (ex != null)
                {
                    var logger = _factory.CreateLogger("Global Exception Handling");
                    
                    object err;

                    if (ex.Error is InvalidOperationException || ex.Error is ResolutionFailedException)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        var msg = "Customer code is not found";
                        LoggerExtensions.LogError(logger, new EventId(1), ex.Error, msg);
                        err = new
                        {
                            Message = msg,
                        };
                    }
                    else
                    {
                        LoggerExtensions.LogError(logger, new EventId(1), ex.Error, "Unhandled exception occurs");

                        err = new
                        {
                            Message = ex.Error.Message,
                            StackTrace = ex.Error.StackTrace
                        };
                    }

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(err)).ConfigureAwait(false);
                }
            });
        }
    }
}