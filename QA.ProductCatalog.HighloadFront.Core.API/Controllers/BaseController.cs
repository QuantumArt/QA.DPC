using System;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Elastic;
using NLog;
using NLog.Fluent;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    public class BaseController : Controller
    {
        
        protected ProductManager Manager { get; }

        protected Logger Logger { get; }

        protected ElasticConfiguration Configuration { get; }

        public BaseController(ProductManager manager, ElasticConfiguration configuration)
        {
            Manager = manager;
            Logger =  LogManager.GetLogger(GetType().FullName);
            Configuration = configuration;
        }

        protected void LogException(Exception ex, string message, params object[] args)
        {
            var builder = Logger.Log(LogLevel.Error).Message(message, args);
            
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
            builder.Write();
        }
    }
}