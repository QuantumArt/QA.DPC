using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader;
using QA.Core.DPC.QP.Services;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.TmForum.Interfaces;
using Article = QA.Core.Models.Entities.Article;
using ContentService = Quantumart.QP8.BLL.Services.API.ContentService;
using FieldService = Quantumart.QP8.BLL.Services.API.FieldService;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;

namespace QA.ProductCatalog.TmForum.Services
{
    public class TmfProductService : JsonProductService
    {
        private readonly IProductAddressProvider _productAddressProvider;
        private readonly string _tmfIdFieldName;

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
            ISettingsService settingsService,
            IProductAddressProvider productAddressProvider)
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
            _tmfIdFieldName = settingsService.GetSetting(SettingsTitles.TMF_ID_FIELD_NAME);
            _productAddressProvider = productAddressProvider;
        }

        protected override void AssignField(Dictionary<string, object> dict, string name, object value)
        {
            string fieldName = name.Equals(_tmfIdFieldName, StringComparison.OrdinalIgnoreCase)
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
                var resourceUri = _productAddressProvider.GetProductAddress(article.ContentDisplayName, resourceId.ToString());
                if (resourceUri is not null)
                {
                    convertedArticle["href"] = resourceUri.AbsoluteUri;
                }
            }

            convertedArticle["lastUpdate"] = article.Modified;

            return convertedArticle;
        }

        protected override IProductDataSource CreateDataSource(IDictionary<string, JToken> tokensDict) =>
            new TmfProductDataSource(tokensDict);
    }
}
