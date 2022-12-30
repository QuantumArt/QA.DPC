using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Generic;
using Article = QA.Core.Models.Entities.Article;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using FieldService = Quantumart.QP8.BLL.Services.API.FieldService;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace QA.Core.DPC.Loader
{
    public class TmfProductService : JsonProductService
    {
        public const string ApiPrefix = "api";

        private static readonly PathString _apiPathPrefix =
            "/" + ApiPrefix.TrimStart('/');

        private readonly IHttpContextAccessor _httpContextAccessor;

        public TmfProductService(
            IConnectionProvider connectionProvider,
            ILogger logger,
            ContentService contentService,
            FieldService fieldService,
            VirtualFieldContextService virtualFieldContextService,
            IRegionTagReplaceService regionTagReplaceService,
            IOptions<LoaderProperties> loaderProperties,
            IHttpClientFactory factory,
            JsonProductServiceSettings settings,
            IHttpContextAccessor httpContextAccessor)
            : base(
                  connectionProvider,
                  logger,
                  contentService,
                  fieldService,
                  virtualFieldContextService,
                  regionTagReplaceService,
                  loaderProperties,
                  factory,
                  settings)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void AssignField(Dictionary<string, object> dict, string name, object value)
        {
            string fieldName = name.Equals(TmfProductDeserializer.TmfIdFieldName, StringComparison.OrdinalIgnoreCase)
                ? nameof(Article.Id)
                : name;

            base.AssignField(dict, fieldName, value);
        }

        public override Dictionary<string, object> ConvertArticle(Article article, IArticleFilter filter)
        {
            var convertedArticle = base.ConvertArticle(article, filter);

            if (convertedArticle is null)
            {
                return convertedArticle;
            }

            convertedArticle = new Dictionary<string, object>(convertedArticle, StringComparer.OrdinalIgnoreCase);

            if (article is null)
            {
                return convertedArticle;
            }

            bool hasType = !string.IsNullOrEmpty(article.ContentDisplayName);
            if (hasType)
            {
                convertedArticle["@type"] = article.ContentDisplayName;
            }

            if (hasType && convertedArticle.TryGetValue(nameof(Article.Id), out var resourceId))
            {
                var resourceUri = GetResourceUri(article.ContentDisplayName, resourceId.ToString());
                if (resourceUri is not null)
                {
                    convertedArticle["href"] = resourceUri.AbsoluteUri;
                }
            }

            convertedArticle["lastUpdate"] = article.Modified;

            return convertedArticle;
        }

        private Uri GetResourceUri(string type, string resourceId)
        {
            var request = _httpContextAccessor.HttpContext.Request;

            (string customerCode, string version, _) = GetTmfRouteValues(request.RouteValues);

            var builder = CreateBuilderFromRequestHost(request);
            builder.Path = _apiPathPrefix + new PathString($"/{customerCode}/{version}/{type}/{resourceId}");

            return builder.Uri;
        }

        private static UriBuilder CreateBuilderFromRequestHost(HttpRequest request) =>
            request.Host.Port.HasValue
                ? new UriBuilder(request.Scheme, request.Host.Host, request.Host.Port.Value)
                : new UriBuilder(request.Scheme, request.Host.Host);

        private static (string customerCode, string version, string slug) GetTmfRouteValues(
            IReadOnlyDictionary<string, object> routeValues)
        {
            var customerCode = GetRouteString(routeValues, "customerCode");
            var version = GetRouteString(routeValues, "version");
            var slug = GetRouteString(routeValues, "slug");

            return (customerCode, version, slug);
        }

        private static string GetRouteString(IReadOnlyDictionary<string, object> routeValues, string name) =>
            TryGetRouteValue<string>(routeValues, name, out var value)
                ? value
                : throw new InvalidOperationException($"Missing mandatory route value {name}.");

        private static bool TryGetRouteValue<TValue>(IReadOnlyDictionary<string, object> routeValues, string name, out TValue typedValue)
        {
            var isSuccess = routeValues.TryGetValue(name, out object value) && value is TValue;
            typedValue = (TValue)value;
            return isSuccess;
        }

        protected override IProductDataSource CreateDataSource(IDictionary<string, JToken> tokensDict) =>
            new TmfProductDataSource(tokensDict);
    }
}
