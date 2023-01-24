using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Filters;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.TmForum.Filters;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Services;
using System.Data;
using System.Net.Mime;

namespace QA.ProductCatalog.TmForum.Controllers
{
    [ApiController]
    [IdentityResolver]
    [TmfProductFormat]
    [Route(TmfProductService.ApiPrefix + "/{customerCode}/{version}/{slug}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class TmfProductController : Controller
    {
        private readonly ILogger _logger;
        private readonly ITmfService _tmfService;

        public TmfProductController(
            ILogger logger,
            TmfService tmfService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tmfService = tmfService;
        }

        /// <summary>
        /// Get list of products
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="lastUpdate">Last modified date filter</param>
        /// <returns>Product list (untyped)</returns>
        /// <response code="406">Unsupported format</response>
        [HttpGet]
        public IActionResult List(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromQuery] string lastUpdate = default)
        {
            _ = _logger.LogDebug(() => new { version, slug, Request.QueryString }.ToString());
            
            SetHttpContextItems();

            if (!TryParseLastUpdateDate(lastUpdate, out var lastUpdateDate))
            {
                return BadRequest();
            }

            var products = _tmfService.GetProducts(slug, version, lastUpdateDate, Request.Query);

            return Ok(products);
        }

        /// <summary>
        /// Get product info
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to get details for</param>
        /// <returns>Product details</returns>
        /// <response code="406">Unsupported format</response>
        /// <response code="404">Product not found</response>
        /// <response code="200">Product retrieved successfully</response>
        [HttpGet("{tmfProductId}")]
        public IActionResult Get(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId, Request.QueryString }.ToString());

            SetHttpContextItems();

            var product = _tmfService.GetProductById(slug, version, tmfProductId);

            if (product is null)
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

            SetHttpContextItems();

            var deleted = _tmfService.TryDeleteProductById(slug, version, tmfProductId);

            if (!deleted)
            {
                return NotFound();
            }

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

            SetHttpContextItems();

            var modifiedProduct = _tmfService.TryUpdateProductById(slug, version, tmfProductId, product);

            if (modifiedProduct is null)
            {
                return NotFound();
            }

            return Ok(modifiedProduct);
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

            SetHttpContextItems();

            var createdProduct = _tmfService.TryCreateProduct(slug, version, product);

            if (createdProduct is null)
            {
                return BadRequest();
            }

            var createdTmfId = ((PlainArticleField)createdProduct.Fields[_tmfService.TmfIdFieldName]).Value;

            return Created(HttpContext.Request.Path + new PathString("/" + createdTmfId), createdProduct);
        }

        private void SetHttpContextItems()
        {
            HttpContext.Items["ArticleFilter"] = ArticleFilter.DefaultFilter;
            HttpContext.Items["includeRegionTags"] = false;
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
    }
}
