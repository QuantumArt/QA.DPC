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
using Microsoft.Extensions.DependencyInjection;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.QP.Models;
using System.Linq;

namespace QA.DPC.Core.Helpers
{
    public class GlobalExceptionHandler
    {
        private readonly ILoggerFactory _factory;
        private readonly string _errorPage;

        public GlobalExceptionHandler(ILoggerFactory factory, string errorPage = null)
        {
            _factory = factory;
            _errorPage = errorPage;
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

                    var provider = options.ApplicationServices.GetService<IIdentityProvider>();
                    var customerCode = provider.Identity?.CustomerCode;

                    if (IsConsolidationError(options, out CustomerState state))
                    {
                        var url = $"{context.Request.PathBase}{_errorPage}?state={state}";
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

                    if (IsConsolidationError(options, out CustomerState state))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        var msg = $"Customer code is not available because of its state {state}";
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

        private bool  IsConsolidationError(IApplicationBuilder options, out CustomerState customerState)
        {
            var provider = options.ApplicationServices.GetService<IIdentityProvider>();
            var customerCode = provider.Identity?.CustomerCode;
            
            if (customerCode == null || customerCode == SingleCustomerCoreProvider.Key)
            {
                customerState = CustomerState.NotDefined;
                return false;

            }
            else
            {
                var factory = options.ApplicationServices.GetService<IFactory>();
                if (factory.NotConsolidatedCodes.Contains(customerCode))
                {
                    customerState = CustomerState.NotRegistered;
                }
                else if (factory.CustomerMap.TryGetValue(customerCode, out CustomerContext context))
                {
                    customerState = context.State;
                    return context.State != CustomerState.Active;
                }
                else
                {
                    customerState = CustomerState.NotFound;
                }

                return true;
            }

        }
    }
}