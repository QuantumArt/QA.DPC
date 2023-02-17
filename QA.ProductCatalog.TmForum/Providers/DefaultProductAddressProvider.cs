using Microsoft.AspNetCore.Http;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Providers
{
    public class DefaultProductAddressProvider : IProductAddressProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultProductAddressProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Uri GetProductAddress(string type, string resourceId)
        {
            HttpRequest request = _httpContextAccessor.HttpContext.Request;

            (string customerCode, string version, _) = GetTmfRouteValues(request.RouteValues);

            UriBuilder builder = CreateBuilderFromRequestHost(request);
            builder.Path = InternalTmfSettings.ApiPathPrefix + new PathString($"/{customerCode}/{version}/{type}/{resourceId}");

            return builder.Uri;
        }

        private static UriBuilder CreateBuilderFromRequestHost(HttpRequest request)
        {
            return request.Host.Port.HasValue
                ? new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port.Value)
                : new UriBuilder(request.Scheme, request.Host.Host);
        }

        private static (string customerCode, string version, string slug) GetTmfRouteValues(
            IReadOnlyDictionary<string, object> routeValues)
        {
            string customerCode = GetRouteString(routeValues, "customerCode");
            string version = GetRouteString(routeValues, "version");
            string slug = GetRouteString(routeValues, "slug");

            return (customerCode, version, slug);
        }

        private static string GetRouteString(IReadOnlyDictionary<string, object> routeValues, string name)
        {
            return TryGetRouteValue<string>(routeValues, name, out string value)
                ? value
                : throw new InvalidOperationException($"Missing mandatory route value {name}.");
        }

        private static bool TryGetRouteValue<TValue>(IReadOnlyDictionary<string, object> routeValues, string name, out TValue typedValue)
        {
            bool isSuccess = routeValues.TryGetValue(name, out object value) && value is TValue;
            typedValue = (TValue)value;
            return isSuccess;
        }
    }
}
