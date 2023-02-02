using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
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

        private static readonly ICollection<string> _notUpdatableFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ExternalIdFieldName
        };

        private const string FieldsQueryParameterName = "fields";
        private const string OffsetQueryParameterName = "offset";
        private const string LimitQueryParameterName = "limit";
        private const string LastUpdateParameterName = "lastUpdate";
        private const string ExternalIdFieldName = "id";
        private const string EntitySeparator = ".";

        private static Regex _idRegex = new("(.*)\\([Vv]ersion=(.*)\\)", RegexOptions.Compiled);

        private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly TmfSettings _tmfSettings;

        public string TmfIdFieldName { get; }

        public TmfService(Func<IProductAPIService> databaseProductServiceFactory,
            IContentDefinitionService contentDefinitionService,
            ISettingsService settingsService,
            IOptions<TmfSettings> tmfSettings)
        {
            _databaseProductServiceFactory = databaseProductServiceFactory ?? throw new ArgumentNullException(nameof(databaseProductServiceFactory));
            _contentDefinitionService = contentDefinitionService;
            TmfIdFieldName = settingsService.GetSetting(SettingsTitles.TMF_ID_FIELD_NAME);
            _notUpdatableFields.Add(TmfIdFieldName);
            _tmfSettings = tmfSettings.Value;
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
            TmfProcessResult getProductResult = GetProductById(slug, version, id, out Article product);

            if (getProductResult != TmfProcessResult.Ok)
            {
                return getProductResult;
            }

            _databaseProductServiceFactory().DeleteProduct(slug, version, product.Id);

            return TmfProcessResult.NoContent;
        }

        public TmfProcessResult UpdateProductById(string slug, string version, string tmfProductId, Article product, out Article updatedProduct)
        {
            TmfProcessResult getProductResult = GetProductById(slug, version, tmfProductId, out updatedProduct);

            if (getProductResult != TmfProcessResult.Ok)
            {
                return getProductResult;
            }

            foreach ((string fieldName, ArticleField fieldValue) in product.Fields
                .Where(p => !_notUpdatableFields.Contains(p.Key, StringComparer.OrdinalIgnoreCase)
                    && p.Value is PlainArticleField field 
                    && field.NativeValue != null))
            {
                updatedProduct.Fields[fieldName] = fieldValue;
            }

            _databaseProductServiceFactory().UpdateProduct(slug, version, updatedProduct);

            return TmfProcessResult.Ok;
        }

        public TmfProcessResult CreateProduct(string slug, string version, Article product, out Article createdProduct)
        {
            IProductAPIService dbProductService = _databaseProductServiceFactory();

            int? createdProductId = dbProductService.CreateProduct(slug, version, product);

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

            KeyValuePair<string, StringValues>[] clearedParameters = searchParameters
                .Where(x => !_reservedSearchParameters.Contains(x.Key))
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

        private static bool TryRetrieveParamaterFromQuery<T>(IQueryCollection query, string parameterName, T defaultValue, out T retrievedValue)
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

        private static bool TryParseParameter<T>(string parameter, T defaultValue, out T result)
        {
            result = defaultValue;

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                
                if (converter is not null)
                {
                    result = (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, parameter);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
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
