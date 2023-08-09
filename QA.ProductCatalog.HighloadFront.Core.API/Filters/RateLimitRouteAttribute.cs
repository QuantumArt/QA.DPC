using Microsoft.AspNetCore.Mvc.Filters;
using QA.DotNetCore.Caching.Interfaces;
using QA.ProductCatalog.HighloadFront.Elastic;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{
    public class RateLimitRouteAttribute : RateLimitAttribute
    {
        public RateLimitRouteAttribute(
            ElasticConfiguration configuration,
            ICacheProvider cacheProvider,
            string profile)
            : base(configuration, cacheProvider, profile)
        {
            
        }
        
        public override string GetActualProfile(ActionExecutingContext context)
        {
            return context.RouteData.Values[Profile].ToString();
        }
    }
}