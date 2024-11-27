using System;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Elastic;
using NLog;
using NLog.Fluent;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    public abstract class BaseController : Controller
    {
        protected ProductManager Manager { get; }

        protected Logger Logger { get; }

        protected ElasticConfiguration Configuration { get; }

        protected BaseController(ProductManager manager, ElasticConfiguration configuration)
        {
            Manager = manager;
            Logger =  LogManager.GetLogger(GetType().FullName);
            Configuration = configuration;
        }
        
        protected bool TraceApiCalls => Environment.GetEnvironmentVariable("TRACE_API_CALLS") == "1";

        protected void LogException(Exception ex, string message, params object[] args)
        {
            var builder = Logger.ForErrorEvent().Message(message, args);
            
            if (ex is ElasticClientException elex)
            {
                if (elex.Request != null)
                {
                    builder.Property("extra", elex.Request);
                }
                builder.Property("address", elex.BaseUrls);
                var eparams = elex.ElasticRequestParams;
                if (eparams != null)
                {
                    builder.Property("index", eparams.IndexName);
                    builder.Property("uri", eparams.GetUri());
                }
            }
            builder.Log();
        }
    }
}