using Microsoft.AspNetCore.Mvc.Filters;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;

namespace QA.ProductCatalog.HighloadFront.Core.API.Filters
{
    public class RateLimitRouteAttribute : RateLimitAttribute
    {
        public RateLimitRouteAttribute(IElasticConfiguration configuration, IVersionedCacheProvider2 cacheProvider,
            string profile) : base(configuration, cacheProvider, profile)
        {
            
        }
        
        public override string GetActualProfile(ActionExecutingContext context)
        {
            return context.RouteData.Values[Profile].ToString();
        }
    }
}