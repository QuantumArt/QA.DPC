﻿using Microsoft.AspNetCore.Http;
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
            LimitQueryParameterName
        };

        private const string FieldsQueryParameterName = "fields";
        private const string OffsetQueryParameterName = "offset";
        private const string LimitQueryParameterName = "limit";
        private const string ExternalIdFieldName = "id";
        private const string EntitySeparator = ".";

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
            _tmfSettings = tmfSettings.Value;
        }

        public TmfProcessResult GetProductById(string slug, string version, string id, out Article product)
        {
            product = null;
            ServiceDefinition definition = _contentDefinitionService.GetServiceDefinition(slug, version);
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
                var foundProduct = dbProductService.GetProduct(slug, version, artricleId);
                articles.Add(foundProduct);
            }

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

            if (!TryRetrievePagingParamaterFromQuery(query, LimitQueryParameterName, _tmfSettings.DefaultLimit, out int limit))
            {
                return TmfProcessResult.BadRequest;
            }

            if (!TryRetrievePagingParamaterFromQuery(query, OffsetQueryParameterName, 0, out int offset))
            {
                return TmfProcessResult.BadRequest;
            }

            products = new(CalculateObjectSize(totalCount, limit, offset))
            {
                TotalCount = totalCount
            };

            var productIdsToProcess = productIds.Skip(offset).Take(limit);

            foreach (int productId in productIdsToProcess)
            {
                products.Articles.Add(dbProductService.GetProduct(slug, version, productId));
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

        private static bool TryRetrievePagingParamaterFromQuery(IQueryCollection query, string parameterName, int defaultValue, out int retrievedValue)
        {
            if (!query.TryGetValue(parameterName, out var valueString))
            {
                retrievedValue = defaultValue;
                return true;
            }

            if (!int.TryParse(valueString.Single(), out var value))
            {
                retrievedValue = defaultValue;
                return false;
            }

            retrievedValue = value;
            return true;
        }

        private static int CalculateObjectSize(int totalCount, int limit, int offset)
        {
            int productsLeft = totalCount - offset;
            return productsLeft < limit ? productsLeft : limit;
        }
    }
}
