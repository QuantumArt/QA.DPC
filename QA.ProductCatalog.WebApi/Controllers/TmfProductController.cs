using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;

namespace QA.ProductCatalog.WebApi.Controllers
{
    [ApiController]
    [IdentityResolver]
    [TmfProductFormat]
    [Route("api/{customerCode}/{version}/{slug}")]
    [Produces(MediaTypeNames.Application.Json)]
    public class TmfProductController : Controller
    {
        // TODO: Remove duplicated constant TmfId. (TmfProductDeserializer.TmfIdFieldName)
        public const string TmfIdFieldName = "TmfId";

        private static readonly ICollection<string> _reservedSearchParameters = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fields"
        };

        private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly ILogger _logger;

        public TmfProductController(
            Func<IProductAPIService> databaseProductServiceFactory,
            ILogger logger)
        {
            _databaseProductServiceFactory = databaseProductServiceFactory ?? throw new ArgumentNullException(nameof(databaseProductServiceFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get list of products
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="fields">Fields filter</param>
        /// <returns>Product list (untyped)</returns>
        /// <response code="406">Unsupported format</response>
        [HttpGet]
        public IEnumerable<Article> List(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromQuery] string[] fields = null)
        {
            _ = _logger.LogDebug(() => new { version, slug, fields }.ToString());
            var dbProductService = _databaseProductServiceFactory();
            var filter = ConvertToFilter(Request.Query, _reservedSearchParameters);

            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;

            int[] productIds = filter.Count > 0
                ? dbProductService.ExtendedSearchProducts(slug, version, filter)
                : dbProductService.GetProductsList(slug, version)
                    .Select(product => (int)product["id"])
                    .ToArray();

            foreach (var productId in productIds)
            {
                // TODO: Optimize (get all of products at ones)
                yield return dbProductService.GetProduct(slug, version, productId);
            }
        }

        /// <summary>
        /// Get product info
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to get details for</param>
        /// <param name="fields">Fields filter</param>
        /// <returns>Product details</returns>
        /// <response code="406">Unsupported format</response>
        /// <response code="404">Product not found</response>
        /// <response code="200">Product retrieved successfully</response>
        [HttpGet("{tmfProductId}")]
        public IActionResult Get(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId,
            [FromQuery] string[] fields = null)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId, fields }.ToString());

            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = FindProductByTmfId(dbProductService, slug, version, tmfProductId);
            if (foundArticleId is null)
            {
                return NotFound();
            }

            var product = dbProductService.GetProduct(slug, version, foundArticleId.Value);

            return Ok(product);
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to delete</param>
        /// <response code="202">Created or updated</response>
        /// <response code="406">Unsupported format</response>
        [HttpDelete("{tmfProductId}")]
        public IActionResult Delete(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId }.ToString());
            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = FindProductByTmfId(dbProductService, slug, version, tmfProductId);
            if (foundArticleId is null)
            {
                return NotFound();
            }

            dbProductService.DeleteProduct(slug, version, foundArticleId.Value);

            return Ok();
        }

        /// <summary>
        /// Create or update product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to update</param>
        /// <param name="product">Product in json format</param>
        /// <response code="202">Created or updated</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpPatch("{tmfProductId}")]
        public ActionResult Update(string slug, string version, [FromRoute] string tmfProductId, [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId }.ToString());

            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = FindProductByTmfId(dbProductService, slug, version, tmfProductId);
            if (foundArticleId is null)
            {
                return NotFound();
            }

            product.Id = foundArticleId.Value;
            _ = SetPlainFieldValue(product.Fields, TmfIdFieldName, tmfProductId);

            dbProductService.UpdateProduct(slug, version, product);

            return Ok();
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
        /// Create or update product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="product">Product in json format</param>
        /// <response code="202">Created or updated</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(202)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpPost]
        public ActionResult Create(
            string slug,
            string version,
            [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId }.ToString());

            var dbProductService = _databaseProductServiceFactory();

            product.Id = 0;
            _ = SetPlainFieldValue<string>(product.Fields, TmfIdFieldName, null);

            // TODO: Remove id to create new article?
            dbProductService.CreateProduct(slug, version, product);

            return Created(HttpContext.Request.Path + new PathString("/" + product.Id), null);
        }

        private static JObject ConvertToFilter(IQueryCollection searchParameters, ICollection<string> excludedKeys)
        {
            var filter = new JObject();
            foreach (var (key, values) in searchParameters)
            {
                if (!excludedKeys.Contains(key))
                {
                    filter.Add(new JProperty(key, values.Single()));
                }
            }

            return filter;
        }

        private int? FindProductByTmfId(IProductAPIService dbProductService, string slug, string version, string tmfProductId)
        {
            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;

            // TODO: Remove duplicated constant TmfId.
            var foundArticles = dbProductService.SearchProducts(slug, version, $"TmfId={tmfProductId}");

            return foundArticles.Length switch
            {
                1 => foundArticles[0],
                0 => null,
                _ => throw new InvalidOperationException("Found duplicated id value. Ids should be unique."),
            };
        }
    }
}