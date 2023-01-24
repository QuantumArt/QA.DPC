using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.TmForum.Interfaces;

namespace QA.ProductCatalog.TmForum.Services
{
    public class TmfService : ITmfService
    {
        private static readonly ICollection<string> _reservedSearchParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fields",
            "lastUpdate"
        };

        private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly Dictionary<string, string> _fieldToFilterMappings;

        public string TmfIdFieldName { get; }

        public TmfService(Func<IProductAPIService> databaseProductServiceFactory,
            IContentDefinitionService contentDefinitionService,
            ISettingsService settingsService)
        {
            _databaseProductServiceFactory = databaseProductServiceFactory ?? throw new ArgumentNullException(nameof(databaseProductServiceFactory));
            _contentDefinitionService = contentDefinitionService;
            TmfIdFieldName = settingsService.GetSetting(SettingsTitles.TMF_ID_FIELD_NAME);
            _fieldToFilterMappings = new()
            {
                [TmfIdFieldName] = nameof(Article.Id)
            };
        }

        public Article GetProductById(string slug, string version, string id)
        {
            IProductAPIService dbProductService = _databaseProductServiceFactory();

            JObject filter = new()
            {
                { TmfIdFieldName, new JValue(id) }
            };

            int foundArticleId = dbProductService.ExtendedSearchProducts(slug, version, filter).SingleOrDefault();

            if (foundArticleId == 0)
            {
                return null;
            }

            Article product = dbProductService.GetProduct(slug, version, foundArticleId);

            return product;
        }

        public List<Article> GetProducts(string slug, string version, DateTime lastUpdateDate, IQueryCollection query)
        {
            IProductAPIService dbProductService = _databaseProductServiceFactory();

            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version);

            JObject filter = ConvertToFilter(query, _reservedSearchParameters, definition.Content.Fields);

            int[] productIds = filter.Count > 0
                ? dbProductService.ExtendedSearchProducts(slug, version, filter)
                : dbProductService.GetProductsList(slug, version)
                    .Select(product => (int)product["id"])
                    .ToArray();

            List<Article> products = new(productIds.Length);
            foreach (int productId in productIds)
            {
                // TODO: Optimize (get all of products at ones)
                Article product = dbProductService.GetProduct(slug, version, productId);

                if (lastUpdateDate != default && product.Modified != lastUpdateDate)
                {
                    continue;
                }

                products.Add(product);
            }

            return products;
        }

        public bool TryDeleteProductById(string slug, string version, string id)
        {
            IProductAPIService dbProductService = _databaseProductServiceFactory();
            int? foundArticleId = ResolveProductId(dbProductService, slug, version, id);

            if (foundArticleId is null)
            {
                return false;
            }

            dbProductService.DeleteProduct(slug, version, foundArticleId.Value);

            return true;
        }

        public Article TryUpdateProductById(string slug, string version, string tmfProductId, Article product)
        {
            var dbProductService = _databaseProductServiceFactory();
            var foundArticleId = ResolveProductId(dbProductService, slug, version, tmfProductId);

            if (foundArticleId is null)
            {
                return null;
            }

            var modifiedProduct = dbProductService.GetProduct(slug, version, foundArticleId.Value);

            foreach ((string fieldName, ArticleField fieldValue) in product.Fields
                .Where(p => p.Value is PlainArticleField field && field.NativeValue != null))
            {
                modifiedProduct.Fields[fieldName] = fieldValue;
            }

            modifiedProduct.Id = foundArticleId.Value;
            _ = SetPlainFieldValue(modifiedProduct.Fields, TmfIdFieldName, tmfProductId);

            dbProductService.UpdateProduct(slug, version, modifiedProduct);

            return modifiedProduct;
        }

        public Article TryCreateProduct(string slug, string version, Article product)
        {
            var dbProductService = _databaseProductServiceFactory();

            var createdProductId = dbProductService.CreateProduct(slug, version, product);

            if (!createdProductId.HasValue)
            {
                return null;
            }

            var createdProduct = dbProductService.GetProduct(slug, version, createdProductId.Value);

            return createdProduct;
        }

        private JObject ConvertToFilter(IQueryCollection searchParameters, ICollection<string> excludedKeys, IEnumerable<Field> fields)
        {
            JObject filter = new();

            if (searchParameters.Count == 0)
            {
                return filter;
            }

            Dictionary<string, string> filterToFieldMappings = fields
                .OfType<PlainField>()
                .Select(field => field.FieldName)
                .ToDictionary(
                    fieldName => _fieldToFilterMappings.TryGetValue(fieldName, out string filter) ? filter : fieldName,
                    fieldName => fieldName,
                    StringComparer.OrdinalIgnoreCase);

            foreach ((string key, Microsoft.Extensions.Primitives.StringValues values) in searchParameters)
            {
                if (excludedKeys.Contains(key) ||
                    !filterToFieldMappings.TryGetValue(key, out string fieldName))
                {
                    continue;
                }

                filter.Add(new JProperty(fieldName, values.Single()));
            }

            return filter;
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
    }
}
