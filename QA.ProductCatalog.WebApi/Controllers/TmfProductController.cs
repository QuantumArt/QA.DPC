using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.Loader;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mime;

namespace QA.ProductCatalog.WebApi.Controllers
{
    [ApiController]
    [IdentityResolver]
    [TmfProductFormat]
    [Route(TmfProductService.ApiPrefix + "/{customerCode}/{version}/{slug}")]
    [Produces(MediaTypeNames.Application.Json)]
    public class TmfProductController : Controller
    {
        // TODO: Remove duplicated constant TmfId. (TmfProductDeserializer.TmfIdFieldName)
        public const string TmfIdFieldName = "TmfId";

        private static readonly ICollection<string> _reservedSearchParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fields",
            "lastUpdate"
        };

        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly ILogger _logger;

        public TmfProductController(
            Func<IProductAPIService> databaseProductServiceFactory,
            ILogger logger,
            IContentDefinitionService contentDefinitionService)
        {
            _databaseProductServiceFactory = databaseProductServiceFactory ?? throw new ArgumentNullException(nameof(databaseProductServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contentDefinitionService = contentDefinitionService;
        }

        /// <summary>
        /// Get list of products
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="fields">Fields filter</param>
        /// <param name="lastUpdate">Last modified date filter</param>
        /// <returns>Product list (untyped)</returns>
        /// <response code="406">Unsupported format</response>
        [HttpGet]
        public IActionResult List(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromQuery] string[] fields = null,
            [FromQuery] string lastUpdate = default)
        {
            _ = _logger.LogDebug(() => new { version, slug, fields }.ToString());
            var dbProductService = _databaseProductServiceFactory();

            var definition = _contentDefinitionService.GetServiceDefinition(slug, version);

            if (!TryParseLastUpdateDate(lastUpdate, out var lastUpdateDate))
            {
                return BadRequest();
            }

            JObject filter = ConvertToFilter(Request.Query, _reservedSearchParameters, definition.Content.Fields);
            if (filter == null)
            {
                return BadRequest(ModelState);
            }

            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;

            int[] productIds = filter.Count > 0
                ? dbProductService.ExtendedSearchProducts(slug, version, filter)
                : dbProductService.GetProductsList(slug, version)
                    .Select(product => (int)product["id"])
                    .ToArray();

            List<Article> products = new(productIds.Length);
            foreach (var productId in productIds)
            {
                // TODO: Optimize (get all of products at ones)
                var product = dbProductService.GetProduct(slug, version, productId);

                if (lastUpdateDate != default && product.Modified != lastUpdateDate)
                {
                    continue;
                }

                products.Add(product);
            }

            return Ok(products);
        }

        private static bool TryParseLastUpdateDate(string lastUpdate, out DateTime lastUpdateDate)
        {
            if (lastUpdate is null)
            {
                lastUpdateDate = default;
                return true;
            }

            return DateTime.TryParse(lastUpdate.Trim('"'), out lastUpdateDate);
        }

        /// <summary>
        /// Get product info
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to get details for</param>
        /// <param name="fields">Fields filter</param>
        /// <param name="lastUpdate">Last modified date filter</param>
        /// <returns>Product details</returns>
        /// <response code="406">Unsupported format</response>
        /// <response code="404">Product not found</response>
        /// <response code="200">Product retrieved successfully</response>
        [HttpGet("{tmfProductId}")]
        public IActionResult Get(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId,
            [FromQuery] string[] fields = null,
            [FromQuery] string lastUpdate = default)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId, fields }.ToString());

            if (!TryParseLastUpdateDate(lastUpdate, out var lastUpdateDate))
            {
                return BadRequest();
            }

            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = ResolveProductId(dbProductService, slug, version, tmfProductId);
            if (foundArticleId is null)
            {
                return NotFound();
            }

            var product = dbProductService.GetProduct(slug, version, foundArticleId.Value);

            if (lastUpdateDate != default && product.Modified != lastUpdateDate)
            {
                return NotFound();
            }

            return Ok(product);
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to delete</param>
        /// <response code="204">Successfully deleted</response>
        /// <response code="406">Unsupported format</response>
        [HttpDelete("{tmfProductId}")]
        public IActionResult Delete(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId }.ToString());
            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = ResolveProductId(dbProductService, slug, version, tmfProductId);
            if (foundArticleId is null)
            {
                return NotFound();
            }

            dbProductService.DeleteProduct(slug, version, foundArticleId.Value);

            return NoContent();
        }

        /// <summary>
        /// Update product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to update</param>
        /// <param name="product">Product in json format</param>
        /// <response code="200">Updated</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpPatch("{tmfProductId}")]
        public ActionResult Update(string slug, string version, [FromRoute] string tmfProductId, [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, tmfProductId, productContentId = product.ContentId }.ToString());

            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = ResolveProductId(dbProductService, slug, version, tmfProductId);
            if (foundArticleId is null)
            {
                return NotFound();
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

            return Ok(modifiedProduct);
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

        /// <summary>
        /// Create product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="product">Product in json format</param>
        /// <response code="201">Created</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(201)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpPost]
        public ActionResult Create(
            string slug,
            string version,
            [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId }.ToString());

            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;

            var dbProductService = _databaseProductServiceFactory();

            var createdProductId = dbProductService.CreateProduct(slug, version, product);

            if (!createdProductId.HasValue)
            {
                return BadRequest();
            }

            var createdProduct = dbProductService.GetProduct(slug, version, createdProductId.Value);
            var createdTmfId = ((PlainArticleField)createdProduct.Fields[TmfIdFieldName]).Value;

            return Created(HttpContext.Request.Path + new PathString("/" + createdTmfId), createdProduct);
        }

        private static readonly Dictionary<string, string> _fieldToFilterMappings = new()
        {
            [TmfIdFieldName] = nameof(Article.Id)
        };

        private static JObject ConvertToFilter(IQueryCollection searchParameters, ICollection<string> excludedKeys, IEnumerable<Field> fields)
        {
            var filter = new JObject();

            if (searchParameters.Count == 0)
            {
                return filter;
            }

            var filterToFieldMappings = fields
                .OfType<PlainField>()
                .Select(field => field.FieldName)
                .ToDictionary(
                    fieldName => _fieldToFilterMappings.TryGetValue(fieldName, out var filter) ? filter : fieldName,
                    fieldName => fieldName,
                    StringComparer.OrdinalIgnoreCase);

            foreach (var (key, values) in searchParameters)
            {
                if (excludedKeys.Contains(key) ||
                    !filterToFieldMappings.TryGetValue(key, out var fieldName))
                {
                    continue;
                }

                filter.Add(new JProperty(fieldName, values.Single()));
            }

            return filter;
        }

        private int? ResolveProductId(IProductAPIService dbProductService, string slug, string version, string tmfProductId)
        {
            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;

            // TODO: Remove duplicated constant TmfId.
            var foundArticles = dbProductService.SearchProducts(slug, version, $"{TmfIdFieldName}={tmfProductId}");

            return foundArticles.Length switch
            {
                1 => foundArticles[0],
                0 => null,
                _ => throw new InvalidOperationException("Found duplicated id products. Ids should be unique."),
            };
        }
    }
}
