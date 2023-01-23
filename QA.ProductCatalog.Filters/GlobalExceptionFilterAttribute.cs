using System;
using System.Collections.Generic;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace QA.ProductCatalog.Filters
{
    public class GlobalExceptionFilterAttribute : ExceptionFilterAttribute
    {

        public override void OnException(ExceptionContext context)
        {
            var factory = (ILoggerFactory)context.HttpContext.RequestServices.GetService(typeof(ILoggerFactory));
            var logger = factory.CreateLogger("Global Exception Handling");
            var format = context.RouteData.Values["format"] ?? WebApiConstants.JsonMappingValue;
            var exception = context.Exception;
            logger.LogError(exception, exception.Message);
            if (format.Equals(WebApiConstants.XmlMappingValue) || format.Equals(WebApiConstants.XamlMappingValue))
            {
                string content;
                using (var ms = new MemoryStream())
                {
                    var error = new Error(exception);
                    new XmlSerializer(error.GetType()).Serialize(ms, error);
                    content = Encoding.UTF8.GetString(ms.ToArray());
                }
                context.Result = new ContentResult()
                {
                    ContentType = WebApiConstants.XmlMediaType,
                    Content = content,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            else if (format.Equals(WebApiConstants.BinaryMappingValue))
            {
                using (var ms = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(ms, context.Exception);
                    context.Result = new FileContentResult(ms.ToArray(), WebApiConstants.BinaryMediaType);
                }
            }
            else
            {
                context.Result = new JsonResult(context.Exception)
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            context.ExceptionHandled = true;
        }
    }
}
