using System.Web.Http;

namespace QA.ProductCatalog.WebApi.Filters
{
    public class DynamicRoutePrefixAttribute : RoutePrefixAttribute
    {
        public static string InitialPrefix;
        public override string Prefix => InitialPrefix;
    }
}