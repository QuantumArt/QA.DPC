using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;

namespace QA.ProductCatalog.TmForum.Services
{
    public class TmfService : ITmfService
    {
        private static readonly ICollection<string> _reservedSearchParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            FieldsQueryParameterName,
            OffsetQueryParameterName,
            LimitQueryParameterName,
            LastUpdateParameterName
        };

        private const string FieldsQueryParameterName = "fields";
        private const string OffsetQueryParameterName = "offset";
        private const string LimitQueryParameterName = "limit";
        private const string LastUpdateParameterName = "lastUpdate";
        private const string ExternalIdFieldName = "id";
        private const string EntitySeparator = ".";

        private static readonly Regex _idRegex = new("(.*)\\([Vv]ersion=(.*)\\)", RegexOptions.Compiled);

        private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly TmfSettings _tmfSettings;
        private readonly ILogger<TmfService> _logger;

        public string TmfIdFieldName { get; }

        public TmfService(Func<IProductAPIService> databaseProductServiceFactory,
            IContentDefinitionService contentDefinitionService,
            ISettingsService settingsService,
            IOptions<TmfSettings> tmfSettings,
            ILogger<TmfService> logger)
        {
            _databaseProductServiceFactory = databaseProductServiceFactory ?? throw new ArgumentNullException(nameof(databaseProductServiceFactory));
            _contentDefinitionService = contentDefinitionService;
            TmfIdFieldName = settingsService.GetSetting(SettingsTitles.TMF_ID_FIELD_NAME);
            _tmfSettings = tmfSettings.Value;
            _logger = logger;
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

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version);

            JObject filter = ConvertToFilter(query, definition);

            if (filter is null)
            {
                products = new(0) { TotalCount = 0 };
                return TmfProcessResult.BadRequest;
            }

            int[] productIds = filter.Count > 0
                ? dbProductService.ExtendedSearchProducts(slug, version, filter)
                : dbProductService.GetProductsList(slug, version)
                    .Select(product => (int)product["id"])
                    .ToArray();

            int totalCount = productIds.Length;
            products = new(0);

            if (totalCount == 0)
            {
                return TmfProcessResult.Ok;
            }

            if (!TryRetrieveParamaterFromQuery(query, LimitQueryParameterName, _tmfSettings.DefaultLimit, out int limit))
            {
                return TmfProcessResult.BadRequest;
            }

            if (!TryRetrieveParamaterFromQuery(query, OffsetQueryParameterName, 0, out int offset))
            {
                return TmfProcessResult.BadRequest;
            }

            products = new(CalculateObjectSize(totalCount, limit, offset))
            {
                TotalCount = totalCount
            };

            IEnumerable<int> productIdsToProcess = productIds.Skip(offset).Take(limit);
            bool hasLastUpdate = TryRetrieveParamaterFromQuery(query, LastUpdateParameterName, null, out DateTime? lastUpdate);

            foreach (int productId in productIdsToProcess)
            {
                var product = dbProductService.GetProduct(slug, version, productId);

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
            IProductAPIService dbProductService = _databaseProductServiceFactory();
            int? foundArticleId = ResolveProductId(dbProductService, slug, version, id);

            if (foundArticleId is null)
            {
                return TmfProcessResult.NotFound;
            }

            dbProductService.DeleteProduct(slug, version, foundArticleId.Value);

            return TmfProcessResult.NoContent;
        }

        public TmfProcessResult UpdateProductById(string slug, string version, string tmfProductId, Article product, out Article updatedProduct)
        {
            var dbProductService = _databaseProductServiceFactory();
            var foundArticleId = ResolveProductId(dbProductService, slug, version, tmfProductId);

            if (foundArticleId is null)
            {
                updatedProduct = null;
                return TmfProcessResult.NotFound;
            }

            updatedProduct = dbProductService.GetProduct(slug, version, foundArticleId.Value);

            foreach ((string fieldName, ArticleField fieldValue) in product.Fields
                .Where(p => p.Value is PlainArticleField field && field.NativeValue != null))
            {
                updatedProduct.Fields[fieldName] = fieldValue;
            }

            updatedProduct.Id = foundArticleId.Value;
            _ = SetPlainFieldValue(updatedProduct.Fields, TmfIdFieldName, tmfProductId);

            dbProductService.UpdateProduct(slug, version, updatedProduct);

            return TmfProcessResult.Ok;
        }

        public TmfProcessResult CreateProduct(string slug, string version, Article product, out Article createdProduct)
        {
            var dbProductService = _databaseProductServiceFactory();

            var createdProductId = dbProductService.CreateProduct(slug, version, product);

            if (!createdProductId.HasValue)
            {
                createdProduct = null;
                return TmfProcessResult.BadRequest;
            }

            createdProduct = dbProductService.GetProduct(slug, version, createdProductId.Value);

            return TmfProcessResult.Created;
        }

        private JObject ConvertToFilter(IQueryCollection searchParameters, ServiceDefinition definition)
        {
            JObject filter = new();

            if (searchParameters.Count == 0)
            {
                return filter;
            }

            var clearedParameters = searchParameters
                .Where(x => !_reservedSearchParameters.Contains(x.Key))
                .ToArray();

            foreach (var parameter in clearedParameters)
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
            string[] parameterParts = parameter.Split(EntitySeparator);
            List<string> fieldNames = new();

            foreach (string part in parameterParts)
            {
                Field field = null;

                if (part.Equals(ExternalIdFieldName, StringComparison.OrdinalIgnoreCase))
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

            return string.Join(EntitySeparator, fieldNames);
        }

        private int? ResolveProductId(IProductAPIService dbProductService, string slug, string version, string tmfProductId)
        {
            int[] foundArticles = dbProductService.SearchProducts(slug, version, $"{TmfIdFieldName}={tmfProductId}");

            return foundArticles.Length switch
            {
                1 => foundArticles[0],
                0 => null,
                _ => throw new InvalidOperationException("Found duplicated id products. Ids should be unique."),
            };
        }

        private static bool SetPlainFieldValue<T>(IReadOnlyDictionary<string, ArticleField> fields, string fieldName, T value)
        {
            if (fields is null)
            {
                throw new ArgumentNullException(nameof(fields));
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException($"Cannot be null or empty.", fieldName);
            }

            if (fields.TryGetValue(fieldName, out var idField))
            {
                if (idField is PlainArticleField plainIdField)
                {
                    plainIdField.NativeValue = value;
                    plainIdField.Value = value?.ToString();
                    return true;
                }
            }

            return false;
        }

        private bool TryRetrieveParamaterFromQuery<T>(IQueryCollection query, string parameterName, T defaultValue, out T retrievedValue)
        {
            retrievedValue = defaultValue;
            if (!query.TryGetValue(parameterName, out var valueString))
            {
                return true;
            }

            string parameterValue = valueString.FirstOrDefault();
            parameterValue = parameterValue.Replace("\"", string.Empty);

            if (string.IsNullOrWhiteSpace(parameterValue))
            {
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
                
                if (converter is not null)
                {
                    result = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, parameter);
                    parseResult = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to convert {value} to type {type}. Filter skipped.", parameter, typeof(T).FullName);
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

            int[] foundArticleIds = dbProductService.ExtendedSearchProducts(slug, version, filter);

            if (foundArticleIds.Length == 0)
            {
                return TmfProcessResult.NotFound;
            }

            List<Article> articles = new(foundArticleIds.Length);

            foreach (int artricleId in foundArticleIds)
            {
                Article foundProduct = dbProductService.GetProduct(slug, version, artricleId);
                articles.Add(foundProduct);
            }

            if (articles.Count == 1)
            {
                product = articles.Single();
                return TmfProcessResult.Ok;
            }

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version);
            string versionField = definition.Content.Fields
                .Where(x => string.Equals(x.FieldName, "version", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.FieldName)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(versionField))
            {
                return TmfProcessResult.BadRequest;
            }

            product = articles.MaxBy(x => x.Fields
                .Where(f => f.Key == versionField)
                .Select(v => v.Value.ToString())
                .Single());

            return TmfProcessResult.Ok;
        }

        private TmfProcessResult GetSpecifiedProductVersionById(string slug, string version, string id, string productVersion, out Article product)
        {
            product = null;
            IProductAPIService dbProductService = _databaseProductServiceFactory();
            JObject filter = new()
            {
                new JProperty(TmfIdFieldName, id)
            };

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version);
            string versionField = definition.Content.Fields
                .Where(x => string.Equals(x.FieldName, "version", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.FieldName)
                .FirstOrDefault();

            filter.Add(new JProperty(versionField, productVersion));

            int[] foundArticleIds = dbProductService.ExtendedSearchProducts(slug, version, filter);

            switch (foundArticleIds.Length)
            {
                case 0:
                    return TmfProcessResult.NotFound;
                case 1:
                    product = dbProductService.GetProduct(slug, version, foundArticleIds.Single());
                    return TmfProcessResult.Ok;
                default:
                    return TmfProcessResult.BadRequest;
            }
        }

        private static TmfProcessResult ParseId(string productId, out string id, out string version)
        {
            id = string.Empty;
            version = string.Empty;

            Match matchResult = _idRegex.Match(productId);

            if (matchResult == null || !matchResult.Success)
            {
                id = productId;
                return TmfProcessResult.Ok;
            }

            string[] keys = matchResult.Groups.Values
                .Where(x => x.Value != productId)
                .Select(x => x.Value)
                .ToArray();

            if (keys.Length != 2)
            {
                return TmfProcessResult.BadRequest;
            }

            id = keys[0];
            version = keys[1];

            if (string.IsNullOrWhiteSpace(id))
            {
                return TmfProcessResult.BadRequest;
            }

            return TmfProcessResult.Ok;
        }
    }
}
