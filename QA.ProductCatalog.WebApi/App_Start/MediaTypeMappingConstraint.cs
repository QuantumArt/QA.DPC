using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public class MediaTypeMappingConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (values.TryGetValue(routeKey, out object value))
            {
                return WebApiConfig.MappingsValues.Any(v => v.Equals(value));
            }

            return false;
        }
    }
}