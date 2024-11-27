using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NLog;
using NLog.Fluent;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;

namespace QA.DPC.Core.Helpers
{
    public class GlobalExceptionHandler
    {
        private readonly string _errorPage;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public GlobalExceptionHandler(string errorPage = null)
        {
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
                    Logger.ForErrorEvent()
                        .Message("Unhandled exception occurs")
                        .Exception(ex.Error)
                        .Property("httpContext", context)
                        .Log();

                    if (IsConsolidationError(options, context, out CustomerState state))
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
                    object err;

                    if (IsConsolidationError(options, context, out CustomerState state))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        var msg = $"Customer code is not available because of its state {state}";
                        err = new
                        {
                            Message = msg,
                        };
                    }
                    else
                    {
                        Logger.ForErrorEvent()
                            .Message("Unhandled exception occurs")
                            .Exception(ex.Error)
                            .Property("httpRequest", context.Request)
                            .Log();

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

        private bool IsConsolidationError(IApplicationBuilder options, HttpContext context, out CustomerState customerState)
        {
            var provider = options.ApplicationServices.GetService<IIdentityProvider>();
            if (provider.IsFixed)
            {
                Logger.ForDebugEvent()
                    .Message("Customer code is fixed")
                    .Property("httpRequest", context.Request)
                    .Log();
                customerState = CustomerState.NotDefined;
                return false;
            }

            var customerCode = provider?.Identity?.CustomerCode;
            if (customerCode == null || customerCode == SingleCustomerCoreProvider.Key)
            {
                Logger.ForDebugEvent()
                    .Message("Customer code is not defined")
                    .Property("httpRequest", context.Request)
                    .Log();
                customerState = CustomerState.NotDefined;
                return false;
            }

            var factory = options.ApplicationServices.GetRequiredService<IFactory>();
            
            if (factory == null)
            {
                customerState = CustomerState.NotDefined;
                return false;
            }
            
            if (factory.NotConsolidatedCodes.Contains(customerCode))
            {
                Logger.ForDebugEvent()
                    .Message("Customer code {customerCode} is not consolidated", customerCode)
                    .Property("httpRequest", context.Request)
                    .Log();
                customerState = CustomerState.NotRegistered;
                return true;
            }

            if (factory.CustomerMap.TryGetValue(customerCode, out CustomerContext customerContext))
            {
                Logger.ForDebugEvent()
                    .Message("Customer code {customerCode} is not active", customerCode)
                    .Property("httpRequest", context.Request)
                    .Log();
                customerState = customerContext.State;
                return customerContext.State != CustomerState.Active;
            }

            Logger.ForDebugEvent()
                .Message("Customer code {customerCode} is not found", customerCode)
                .Property("httpRequest", context.Request)
                .Log();
            customerState = CustomerState.NotFound;
            return true;
        }
    }
}