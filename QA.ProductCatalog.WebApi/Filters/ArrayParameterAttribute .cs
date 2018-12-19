using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class ArrayParameterAttribute : ActionFilterAttribute
    {
        private readonly string _parameterName;

        public ArrayParameterAttribute(string parameterName)
        {
            _parameterName = parameterName;
            Separator = ',';
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ActionArguments.ContainsKey(_parameterName))
            {
                var rr = HttpUtility.ParseQueryString("");

                string parameters = string.Empty;

                if (actionContext.ControllerContext.RouteData.Values.ContainsKey(_parameterName))
                {
                    parameters = (string)actionContext.ControllerContext.RouteData.Values[_parameterName] ?? string.Empty;
                }
                else
                {
                    parameters = HttpUtility.ParseQueryString(actionContext.ControllerContext.Request.RequestUri.ToString())[_parameterName] ?? string.Empty;
                }

                actionContext.ActionArguments[_parameterName] = parameters.Split(Separator).Select(int.Parse).ToArray();
            }
        }

        public char Separator { get; set; }
    }
}