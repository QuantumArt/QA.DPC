using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Options;
using ResponseCacheLocation = Microsoft.AspNetCore.Mvc.ResponseCacheLocation;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    public class BaseController : Controller
    {
        
        protected ProductManager Manager { get; }

        protected ILogger Logger  { get; }

        protected ElasticConfiguration Configuration { get; }

        public BaseController(ProductManager manager, ILoggerFactory loggerFactory, ElasticConfiguration configuration)
        {
            Manager = manager;
            Logger = loggerFactory.CreateLogger(GetType());
            Configuration = configuration;
        }
        
        protected void Log(LogLevel level, string message, params object[] args)
        {
            LogExtra(level, message, null, null, null, args);
        }

        private void LogExtra(LogLevel level, string message, ElasticRequestParams eparams, Dictionary<string, object> extra, Exception ex, params object[] args)
        {
            
            var evt = new CustomLogEvent(message, args);
            if (extra != null)
            {
                foreach (var item in extra)
                {
                    evt.AddProp(item.Key, item.Value);
                }
            }

            if (eparams != null)
            {
                evt.AddProp("index", eparams.IndexName);
                evt.AddProp("uri", eparams.GetUri());
            }
            
            Logger.Log(level, default(EventId), evt, ex, CustomLogEvent.Formatter);
        }

        protected void LogException(Exception ex, string message, params object[] args)
        {
            var extra = new Dictionary<string, object>();

            ElasticRequestParams eparams = null;
            if (ex is ElasticClientException elex)
            {
                if (elex.Request != null)
                {
                    extra.Add("extra", elex.Request);
                }
                extra.Add("address", elex.BaseUrls);
                eparams = elex.ElasticRequestParams;
            }

            LogExtra(LogLevel.Error, message, eparams, extra, ex, args);
        }
    }
}