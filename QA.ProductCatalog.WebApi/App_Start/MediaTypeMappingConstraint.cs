using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace QA.ProductCatalog.WebApi.App_Start
{
    public class MediaTypeMappingConstraint : IHttpRouteConstraint
    {
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (values.TryGetValue(parameterName, out object value))
            {
                return WebApiConfig.MappingsValues.Any(v => v.Equals(value));
            }

            return false;
        }
    }
}