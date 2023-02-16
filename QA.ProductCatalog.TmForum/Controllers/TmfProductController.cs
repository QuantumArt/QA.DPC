using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QA.Core.Logger;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Filters;
using QA.ProductCatalog.TmForum.Filters;
using QA.ProductCatalog.TmForum.Interfaces;
using QA.ProductCatalog.TmForum.Models;
using System.Net.Mime;

namespace QA.ProductCatalog.TmForum.Controllers
{
    [ApiController]
    [IdentityResolver]
    [TmfProductFormat]
    [Route(InternalTmfSettings.ApiPrefix + "/{customerCode}/{version}/{slug}")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class TmfProductController : Controller
    {
        private readonly ILogger _logger;
        private readonly ITmfService _tmfService;

        public TmfProductController(
            ILogger logger,
            ITmfService tmfService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tmfService = tmfService;
        }

        /// <summary>
        /// Get list of products
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <returns>Product list (untyped)</returns>
        /// <response code="406">Unsupported format</response>
        /// <response code="200">Ok</response>
        /// <response code="206">Partial content</response>
        /// <response code="400">Bad request</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status206PartialContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpGet]
        public IActionResult List(
            [FromRoute] string slug,
            [FromRoute] string version)
        {
            _ = _logger.LogDebug(() => new { version, slug, Request.QueryString }.ToString());

            var result = _tmfService.GetProducts(slug, version, Request.Query, out var products);

            return GenerateResult(result, products.Articles, products.TotalCount);
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpGet("{tmfProductId}")]
        public IActionResult Get(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId, Request.QueryString }.ToString());

            var result = _tmfService.GetProductById(slug, version, tmfProductId, out var product);

            return GenerateResult(result, product);
        }

        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to delete</param>
        /// <response code="204">Successfully deleted</response>
        /// <response code="404">Product not found</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpDelete("{tmfProductId}")]
        public IActionResult Delete(
            [FromRoute] string slug,
            [FromRoute] string version,
            [FromRoute] string tmfProductId)
        {
            _ = _logger.LogDebug(() => new { version, slug, tmfProductId }.ToString());

            var result = _tmfService.DeleteProductById(slug, version, tmfProductId);

            return GenerateResult(result);
        }

        /// <summary>
        /// Update product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. productSpecification)</param>
        /// <param name="version">Definition version (e.g. v1)</param>
        /// <param name="tmfProductId">Identifier of product to update</param>
        /// <param name="product">Product in json format</param>
        /// <response code="200">Updated</response>
        /// <response code="404">Product not found</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpPatch("{tmfProductId}")]
        public IActionResult Update(string slug, string version, [FromRoute] string tmfProductId, [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, tmfProductId, productContentId = product.ContentId }.ToString());

            var result = _tmfService.UpdateProductById(slug, version, tmfProductId, product, out ResultArticle resultProduct);

            return GenerateResult(result, result == TmfProcessResult.BadRequest ? resultProduct.ValidationErrors : resultProduct.Article);
        }

        /// <summary>
        /// Create product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="product">Product in json format</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad request</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
        [HttpPost]
        public IActionResult Create(
            string slug,
            string version,
            [FromBody] Article product)
        {
            _ = _logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId }.ToString());

            var result = _tmfService.CreateProduct(slug, version, product, out ResultArticle resultProduct);

            return GenerateResult(result, result == TmfProcessResult.BadRequest ? resultProduct.ValidationErrors : resultProduct.Article);
        }

#nullable enable
        private IActionResult GenerateResult(TmfProcessResult result, object? resultObject = null, int totalCount = 0)
        {
            switch (result)
            {
                case TmfProcessResult.Ok:
                    return Ok(resultObject);
                case TmfProcessResult.NotFound:
                    return NotFound();
                case TmfProcessResult.BadRequest:
                    return BadRequest(resultObject);
                case TmfProcessResult.Created:
                    var createdTmfId = ((PlainArticleField)((Article)resultObject!).Fields[_tmfService.TmfIdFieldName]).Value;
                    var path = HttpContext.Request.Path.Add(new PathString($"/{createdTmfId}"));
                    return Created(path, resultObject);
                case TmfProcessResult.NoContent:
                    return NoContent();
                case TmfProcessResult.PartialContent:
                    Response.Headers.Add("X-Total-Count", totalCount.ToString());
                    Response.StatusCode = StatusCodes.Status206PartialContent;
                    return new ObjectResult(resultObject);
                default:
                    throw new ArgumentException($"Not supported result {result}");
            }
        }
    }
}
