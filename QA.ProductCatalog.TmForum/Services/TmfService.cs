using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.TmForum.Extensions;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;
using BLL = Quantumart.QP8.BLL;

namespace QA.ProductCatalog.TmForum.Services
{
    public class TmfService : ITmfService
    {
        private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly TmfSettings _tmfSettings;
        private readonly ILogger<TmfService> _logger;
        private readonly IArticleFormatter _formatter;
        private readonly IJsonProductService _jsonProductService;
        private readonly JsonMergeSettings _jsonMergeSettings;
        private readonly ITmfValidatonService _tmfValidationService;
        private readonly IHttpContextAccessor _contextAccessor;
        public string TmfIdFieldName { get; }

        public TmfService(Func<IProductAPIService> databaseProductServiceFactory,
            IContentDefinitionService contentDefinitionService,
            ISettingsService settingsService,
            IOptions<TmfSettings> tmfSettings,
            ILogger<TmfService> logger,
            IArticleFormatter formatter,
            IJsonProductService jsonProductService,
            ITmfValidatonService tmfValidationService,
            IHttpContextAccessor contextAccessor)
        {
            _databaseProductServiceFactory = databaseProductServiceFactory ?? throw new ArgumentNullException(nameof(databaseProductServiceFactory));
            _contentDefinitionService = contentDefinitionService;
            TmfIdFieldName = settingsService.GetSetting(SettingsTitles.TMF_ID_FIELD_NAME);
            _tmfSettings = tmfSettings.Value;
            _logger = logger;
            _formatter = formatter;
            _jsonProductService = jsonProductService;
            _jsonMergeSettings = new()
            {
                MergeArrayHandling = MergeArrayHandling.Replace,
                MergeNullValueHandling = MergeNullValueHandling.Merge,
                PropertyNameComparison = StringComparison.OrdinalIgnoreCase
            };
            _tmfValidationService = tmfValidationService;
            _contextAccessor = contextAccessor;
        }

        public TmfProcessResult GetProductById(string slug, string version, string id, out Article product)
        {
            product = null;
            TmfProcessResult parseResult = ParseId(id, out string productId, out string productVersion);

            if (parseResult != TmfProcessResult.Ok)
            {
                return parseResult;
            }

            if (string.IsNullOrWhiteSpace(productVersion))
            {
                return GetLatestProductVersionById(slug, version, productId, out product);
            }

            return GetSpecifiedProductVersionById(slug, version, productId, productVersion, out product);
        }

        public TmfProcessResult GetProducts(string slug, string version, IQueryCollection query, out ArticleList products)
        {
            IProductAPIService dbProductService = _databaseProductServiceFactory();

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version, true);

            JObject filter = ConvertToFilter(query, definition);

            if (filter is null)
            {
                products = new(0) { TotalCount = 0 };
                return TmfProcessResult.BadRequest;
            }

            int[] productIds = filter.Count > 0
                ? dbProductService.ExtendedSearchProducts(slug, version, filter, _tmfSettings.IsLive)
                : dbProductService.GetProductsList(slug, version, _tmfSettings.IsLive)
                    .Select(product => (int)product["id"])
                    .ToArray();

            int totalCount = productIds.Length;
            products = new(0);

            if (totalCount == 0)
            {
                return TmfProcessResult.Ok;
            }

            if (!TryRetrieveParameterFromQuery(query, InternalTmfSettings.LimitQueryParameterName, _tmfSettings.DefaultLimit, out int limit))
            {
                return TmfProcessResult.BadRequest;
            }

            if (!TryRetrieveParameterFromQuery(query, InternalTmfSettings.OffsetQueryParameterName, 0, out int offset))
            {
                return TmfProcessResult.BadRequest;
            }

            products = new(CalculateObjectSize(totalCount, limit, offset))
            {
                TotalCount = totalCount
            };

            IEnumerable<int> productIdsToProcess = productIds.Skip(offset).Take(limit);
            bool hasLastUpdate = TryRetrieveParameterFromQuery(query,
                InternalTmfSettings.LastUpdateParameterName,
                null,
                out DateTime? lastUpdate);

            foreach (int productId in productIdsToProcess)
            {
                Article product = dbProductService.GetProduct(slug, version, productId, _tmfSettings.IsLive);

                if (hasLastUpdate && lastUpdate != null && product.Modified != lastUpdate)
                {
                    continue;
                }

                products.Articles.Add(product);
            }

            return products.IsPartial ? TmfProcessResult.PartialContent : TmfProcessResult.Ok;
        }

        public TmfProcessResult DeleteProductById(string slug, string version, string id)
        {
            TmfProcessResult getProductResult = GetProductById(slug, version, id, out Article product);

            if (getProductResult != TmfProcessResult.Ok)
            {
                return getProductResult;
            }

            IProductAPIService productService = _databaseProductServiceFactory();

            productService.DeleteProduct(slug, version, product.Id);
            ExecuteCustomAction(productService, "DeleteAction", product.Id, product.ContentId);

            _logger.LogInformation("Product with id {id} was deleted.", product.Id);

            return TmfProcessResult.NoContent;
        }

        public TmfProcessResult UpdateProductById(string slug, string version, string tmfProductId, Article product, out ResultArticle resultProduct)
        {
            resultProduct = new();

            TmfProcessResult getProductResult = GetProductById(slug, version, tmfProductId, out Article originalProduct);

            if (getProductResult != TmfProcessResult.Ok)
            {
                return getProductResult;
            }

            bool isUpdatable = true;
            _tmfValidationService.CheckArticleState(originalProduct, ref isUpdatable);

            if (!isUpdatable)
            {
                _logger.LogWarning(InternalTmfSettings.ArticleStateErrorText);

                resultProduct.ValidationErrors = InternalTmfSettings.ArticleStateErrorArray;
                return TmfProcessResult.BadRequest;
            }

            string originalJson = _formatter.Serialize(originalProduct);
            _logger.LogTrace("Original product json is: {product}", originalJson);
            string patchJson = _formatter.Serialize(product);
            _logger.LogTrace("Patch product json is: {patch}", patchJson);

            JObject original = JObject.Parse(originalJson);
            JObject patch = JObject.Parse(patchJson);

            patch.RemoveUpdateRestrictedFields();
            original.Merge(patch, _jsonMergeSettings);
            _logger.LogTrace("Product json after merge is: {merged}", original.ToString());

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version, true);
            Article mergedProduct = _jsonProductService.DeserializeProduct(original.ToString(), definition.Content);

            resultProduct.ValidationErrors = ValidateReceivedProduct(mergedProduct);

            if (resultProduct.ValidationErrors.Length > 0)
            {
                return TmfProcessResult.BadRequest;
            }

            IProductAPIService productService = _databaseProductServiceFactory();
            productService.UpdateProduct(slug, version, mergedProduct, _tmfSettings.IsLive);

            TmfProcessResult updateArticleResult = GetProductById(slug, version, tmfProductId, out Article updatedArticle);

            if (updateArticleResult != TmfProcessResult.Ok)
            {
                return updateArticleResult;
            }

            resultProduct.Article = updatedArticle;
            _logger.LogInformation("Product with id {id} updated successfully.", resultProduct.Article.Id);
                
            ExecuteCustomAction(productService,
                "PublishAction",
                resultProduct.Article.Id,
                resultProduct.Article.ContentId);

            return updateArticleResult;
        }

        public TmfProcessResult CreateProduct(string slug, string version, Article product, out ResultArticle resultProduct)
        {
            resultProduct = new()
            {
                ValidationErrors = ValidateReceivedProduct(product)
            };

            if (resultProduct.ValidationErrors.Length > 0)
            {
                return TmfProcessResult.BadRequest;
            }

            IProductAPIService dbProductService = _databaseProductServiceFactory();
            int? createdProductId = dbProductService.CreateProduct(slug, version, product, _tmfSettings.IsLive);

            if (!createdProductId.HasValue)
            {
                _logger.LogWarning("Create product method executed but created product Id is empty.");
                return TmfProcessResult.BadRequest;
            }

            resultProduct.Article = dbProductService.GetProduct(slug, version, createdProductId.Value, _tmfSettings.IsLive);
            _logger.LogInformation("Product with id {id} created.", resultProduct.Article.Id);
            
            ExecuteCustomAction(dbProductService,
                "PublishAction",
                resultProduct.Article.Id,
                resultProduct.Article.ContentId);

            return TmfProcessResult.Created;
        }

        private void ExecuteCustomAction(IProductAPIService service, string actionName, int productId, int contentId)
        {
            _contextAccessor.HttpContext.Items.Remove(InternalTmfSettings.TmfItemIdentifier);
            service.CustomAction(actionName, productId, contentId);
            _contextAccessor.HttpContext.Items[InternalTmfSettings.TmfItemIdentifier] = true;
        }

        private JObject ConvertToFilter(IQueryCollection searchParameters, ServiceDefinition definition)
        {
            JObject filter = new();

            if (searchParameters.Count == 0)
            {
                return filter;
            }

            KeyValuePair<string, StringValues>[] clearedParameters = searchParameters
                .Where(x => !InternalTmfSettings.ReservedSearchParameters.Contains(x.Key))
                .ToArray();

            foreach (KeyValuePair<string, StringValues> parameter in clearedParameters)
            {
                string fieldName = GetFieldNameFromDefinition(definition, parameter.Key);
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    return null;
                }

                filter.Add(new JProperty(fieldName, parameter.Value.Single()));
            }

            return filter;
        }

        private string GetFieldNameFromDefinition(ServiceDefinition definition, string parameter)
        {
            Content content = definition.Content;
            string[] parameterParts = parameter.Split(InternalTmfSettings.EntitySeparator);
            List<string> fieldNames = new();

            foreach (string part in parameterParts)
            {
                Field field = null;

                if (part.Equals(InternalTmfSettings.ExternalIdFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    field = content.Fields.FirstOrDefault(f => string.Equals(f.FieldName, TmfIdFieldName, StringComparison.CurrentCultureIgnoreCase));
                }

                field ??= content.Fields.FirstOrDefault(f => string.Equals(f.FieldName, part, StringComparison.CurrentCultureIgnoreCase));

                if (field is PlainField plainField)
                {
                    fieldNames.Add(plainField.FieldName);
                    break;
                }

                if (field is EntityField entity)
                {
                    fieldNames.Add(entity.FieldName);
                    content = entity.Content;
                    continue;
                }

                break;
            }

            return string.Join(InternalTmfSettings.EntitySeparator, fieldNames);
        }

        private bool TryRetrieveParameterFromQuery<T>(IQueryCollection query,
            string parameterName,
            T defaultValue,
            out T retrievedValue)
        {
            retrievedValue = defaultValue;
            if (!query.TryGetValue(parameterName, out StringValues valueString))
            {
                _logger.LogTrace("Unable to retrieve parameter with name {name} from query. Using default value {default}.", 
                    parameterName, 
                    defaultValue.ToString());

                return true;
            }

            string parameterValue = valueString.FirstOrDefault(string.Empty);
            parameterValue = parameterValue.Replace("\"", string.Empty);

            if (string.IsNullOrWhiteSpace(parameterValue))
            {
                _logger.LogWarning("There is parameter named {name} but it's value is empty.", parameterName);
                return false;
            }

            return TryParseParameter(parameterValue, defaultValue, out retrievedValue);
        }

        private bool TryParseParameter<T>(string parameter, T defaultValue, out T result)
        {
            result = defaultValue;
            bool parseResult = false;

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

                result = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, parameter);
                parseResult = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to convert {value} to type {type}. Filter skipped.",
                    parameter,
                    typeof(T).FullName);
            }

            return parseResult;
        }

        private static int CalculateObjectSize(int totalCount, int limit, int offset)
        {
            int productsLeft = totalCount - offset;
            return productsLeft < limit ? productsLeft : limit;
        }

        private TmfProcessResult GetLatestProductVersionById(string slug, string version, string id, out Article product)
        {
            product = null;
            IProductAPIService dbProductService = _databaseProductServiceFactory();
            JObject filter = new()
            {
                new JProperty(TmfIdFieldName, id)
            };

            int[] foundArticleIds = dbProductService.ExtendedSearchProducts(slug, version, filter, _tmfSettings.IsLive);

            if (foundArticleIds.Length == 0)
            {
                return TmfProcessResult.NotFound;
            }

            List<Article> articles = new(foundArticleIds.Length);

            foreach (int articleId in foundArticleIds)
            {
                Article foundProduct = dbProductService.GetProduct(slug, version, articleId, _tmfSettings.IsLive);
                articles.Add(foundProduct);
            }

            if (articles.Count == 1)
            {
                product = articles.Single();
                return TmfProcessResult.Ok;
            }

            if (!TryGetVersionFieldName(slug, version, out string versionField))
            {
                return TmfProcessResult.BadRequest;
            }

            product = articles.MaxBy(x => x.Fields
                .Where(f => f.Key == versionField)
                .Select(v => v.Value.ToString())
                .Single());

            return TmfProcessResult.Ok;
        }

        private TmfProcessResult GetSpecifiedProductVersionById(string slug,
            string version,
            string id,
            string productVersion,
            out Article product)
        {
            product = null;
            IProductAPIService dbProductService = _databaseProductServiceFactory();
            JObject filter = new()
            {
                new JProperty(TmfIdFieldName, id)
            };

            if (!TryGetVersionFieldName(slug, version, out string versionField))
            {
                return TmfProcessResult.BadRequest;
            }

            filter.Add(new JProperty(versionField, productVersion));

            int[] foundArticleIds = dbProductService.ExtendedSearchProducts(slug, version, filter, _tmfSettings.IsLive);

            switch (foundArticleIds.Length)
            {
                case 0:
                    return TmfProcessResult.NotFound;
                case 1:
                    product = dbProductService.GetProduct(slug, version, foundArticleIds.Single(), _tmfSettings.IsLive);
                    return TmfProcessResult.Ok;
                default:
                    _logger.LogWarning("Found {count} articles by id {id} and version {version}. That's unexpected.",
                        foundArticleIds.Length,
                        id,
                        productVersion);
                    return TmfProcessResult.BadRequest;
            }
        }

        private bool TryGetVersionFieldName(string slug, string version, out string versionField)
        {
            bool result = true;

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version, true);
            versionField = definition.Content.Fields
                .Where(x => string.Equals(x.FieldName, InternalTmfSettings.VersionFieldName, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.FieldName)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(versionField))
            {
                _logger.LogWarning("Unable to find version field name by slug {slug}.", slug);
                result = false;
            }

            return result;
        }

        private TmfProcessResult ParseId(string productId, out string id, out string version)
        {
            id = string.Empty;
            version = string.Empty;

            Match matchResult = InternalTmfSettings.IdRegex.Match(productId);

            if (!matchResult.Success)
            {
                _logger.LogTrace("Id value {id} without version. Proceeding with id as is.", productId);
                id = productId;
                return TmfProcessResult.Ok;
            }

            string[] keys = matchResult.Groups.Values
                .Where(x => x.Value != productId)
                .Select(x => x.Value)
                .ToArray();

            if (keys.Length != 2)
            {
                _logger.LogWarning("Product ID {productId} does not contain id and version numbers.", productId);
                return TmfProcessResult.BadRequest;
            }

            id = keys[0];
            version = keys[1];

            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(version))
            {
                _logger.LogWarning("Either product id {id} or version number {version} is empty in parsed string {productId}.", 
                    id, 
                    version, 
                    productId);

                return TmfProcessResult.BadRequest;
            }

            return TmfProcessResult.Ok;
        }

        private string[] ValidateReceivedProduct(Article product)
        {
            string[] result = Array.Empty<string>();
            BLL.RulesException errors = new();

            _tmfValidationService.ValidateArticle(errors, product);

            if (errors.Errors.Count != 0)
            {
                result = errors.Errors.Select(x => x.Message).ToArray();
                _logger.LogWarning("Product validation failed with errors: {errors}.", string.Join(", ", result));
            }

            return result;
        }
    }
}
