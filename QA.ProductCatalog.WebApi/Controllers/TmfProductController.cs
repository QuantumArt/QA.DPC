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
using System.Diagnostics;
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
        /// <param name="productId">Identifier of product to get details for</param>
        /// <param name="fields">Fields filter</param>
        /// <returns>Product details</returns>
        /// <response code="406">Unsupported format</response>
        /// <response code="404">Product not found</response>
        /// <response code="200">Product retrieved successfully</response>
        [HttpGet("{productId}")]
        public IActionResult Get(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string productId,
            [FromQuery] string[] fields = null)
        {
            _ = _logger.LogDebug(() => new { version, slug, productId, fields }.ToString());
            var stopwatch = Stopwatch.StartNew();
            var dbProductService = _databaseProductServiceFactory();
            Console.WriteLine(stopwatch.Elapsed);

            var foundArticleId = FindProductById(dbProductService, slug, version, productId);
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
        /// <param name="productId">Identifier of product to delete</param>
        /// <response code="202">Created or updated</response>
        /// <response code="406">Unsupported format</response>
        [HttpDelete("{productId}")]
        public IActionResult Delete(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string productId)
        {
            _ = _logger.LogDebug(() => new { version, slug, productId }.ToString());
            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = FindProductById(dbProductService, slug, version, productId);
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
        /// <param name="productId">Identifier of product to update</param>
        /// <param name="product">Product in json format</param>
        /// <response code="202">Created or updated</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(202)]
        [ProducesResponseType(406)]
        [HttpPut("{productId}")]
        public ActionResult Update(string slug, string version, [FromRoute] string productId, [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId }.ToString());
            var dbProductService = _databaseProductServiceFactory();

            var foundArticleId = FindProductById(dbProductService, slug, version, productId);
            if (foundArticleId is null)
            {
                return NotFound();
            }

            product.Id = foundArticleId.Value;
            dbProductService.CreateProduct(slug, version, product);

            return Created(HttpContext.Request.Path + new PathString("/" + product.Id), null);
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
        [ProducesResponseType(406)]
        [HttpPost]
        public ActionResult Create(
            string slug,
            string version,
            [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId }.ToString());
            _databaseProductServiceFactory().UpdateProduct(slug, version, product);

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

        private int? FindProductById(IProductAPIService dbProductService, string slug, string version, string productId)
        {
            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;

            var foundArticles = dbProductService.SearchProducts(slug, version, $"Id={productId}");

            return foundArticles.Length switch
            {
                1 => foundArticles[0],
                0 => null,
                _ => throw new InvalidOperationException("Found duplicated id value. Ids should be unique."),
            };
        }
    }
}