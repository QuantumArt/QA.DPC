using System;
using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Autopublish.Models;
using QA.Core.DPC.QP.Autopublish.Services;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.WebApi.Models;
using QA.ProductCatalog.Filters;

namespace QA.ProductCatalog.WebApi.Controllers
{
    /// <summary>
    /// Product manipulation. Methods support different output formats: json, xml, xaml, pdf, binary
    /// Supply format via 'format' path segment
    /// </summary>
    [IdentityResolver]
    [Route("api")]
    [FormatFilter]
    public class ProductController : Controller
	{
		private readonly Func<IProductAPIService> _databaseProductServiceFactory;
        private readonly IProductSimpleAPIService _tarantoolProductService;
        private readonly IAutopublishProcessor _autopublishProcessor;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _accessor;

		public ProductController(
			Func<IProductAPIService> databaseProductServiceFactory, 
			IProductSimpleAPIService tarantoolProductService, 
			IAutopublishProcessor autopublishProcessor, 
			ILogger logger,
			IHttpContextAccessor accessor
			)
		{
			_databaseProductServiceFactory = databaseProductServiceFactory;
            _tarantoolProductService = tarantoolProductService;
            _autopublishProcessor = autopublishProcessor;
            _logger = logger;
            _accessor = accessor;

		}

        /// <summary>
        /// Get list of products
        /// </summary>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="format">json, xml or binary</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="startRow">Start from product with number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Product list (untyped)</returns>
        /// <response code="406">Unsupported format</response>
        [HttpGet("{version}/{slug}/{format:regex(^json|xml|binary$)}", Name = "List")]
        [HttpGet("{customerCode}/{version}/{slug}/{format:regex(^json|xml|binary$)}", Name = "List-Consolidate")]
        public ActionResult<Dictionary<string, object>[]> List(string version, string slug, string format, 
	        bool isLive = false, long startRow = 0, long pageSize = 10)
		{
			_logger.LogDebug(() => new { version, slug, format, isLive, startRow, pageSize }.ToString());
			var result = _databaseProductServiceFactory().GetProductsList(slug, version, isLive, startRow, pageSize);
			return result;
		}

        /// <summary>
        /// Get list of products by query.
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json, xml, or binary</param> 
        /// <param name="query">A part of SQL query (where clause)</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns>Product ids</returns>
        /// <response code="406">Unsupported format</response>
        [HttpGet("{version}/{slug}/search/{format:regex(^json|xml|binary$)}/{query}", Name = "Search")]
        [HttpGet("{customerCode}/{version}/{slug}/search/{format:regex(^json|xml|binary$)}/{query}", Name = "Search-Consolidate")]
        public ActionResult<int[]> Search(string slug, string version, string format, string query, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, format, query, isLive }.ToString());
			return _databaseProductServiceFactory().SearchProducts(slug, version, query, isLive);
		}

        /// <summary>
        /// Get detailed list of products by query
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json or binary</param>   
        /// <param name="query">A part of SQL query (where clause). Supports only text fields (like Alias).
        /// Use '_' as hierarchy delimiter</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="includeRegionTags">Include or not region tags in the result</param>
        /// <param name="includeRelevanceInfo">Include or not relevance information in the result</param>
        /// <returns>Product list</returns>
        /// <response code="406">Unsupported format</response>

        [HttpGet("{version}/{slug}/search/detail/{format:regex(^json|binary$)}/{query}", Name="SearchDetailed")]
        [HttpGet("{customerCode}/{version}/{slug}/search/detail/{format:regex(^json|binary$)}/{query}", Name="SearchDetailed-Consolidate")]
        
        public ActionResult<IEnumerable<Article>> SearchDetailed(string slug, string version, string format, 
	        string query, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
        {
	        var service = _databaseProductServiceFactory();
            _logger.LogDebug(() => new { slug, version, format, query, isLive }.ToString());
            var ids = service.SearchProducts(slug, version, query, isLive);

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

			var result = new List<Article>();				
            foreach (var id in ids)
            {
	            result.Add(service.GetProduct(slug, version, id, isLive, includeRelevanceInfo));
            }

            return result;
        }

        /// <summary>
        /// Get list of products by extended query.
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json, xml or binary</param>   
        /// <param name="query">A part of SQL query (where clause)</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns>Product ids</returns>
        /// <response code="406">Unsupported format</response>

        [HttpPost("{version}/{slug}/search/{format:regex(^json|xml|binary$)}", Name="ExtendedSearch")]
        [HttpPost("{customerCode}/{version}/{slug}/search/{format:regex(^json|xml|binary$)}", Name="ExtendedSearch-Consolidate")]
        public ActionResult<int[]> ExtendedSearch(string slug, string version, string format, [FromBody] JToken query, bool isLive = false)
        {
            _logger.LogDebug(() => new { slug, version, format, query, isLive }.ToString());
            return _databaseProductServiceFactory().ExtendedSearchProducts(slug, version, query, isLive);
        }

        /// <summary>
        /// Get detailed list of products by extended query.
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json or binary</param> 
        /// <param name="query">A part of SQL query (where clause)</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="includeRegionTags">Include or not region tags in the result</param>
        /// <param name="includeRelevanceInfo">Include or not relevance information in the result</param>
        /// <returns>Product list</returns>
        /// <response code="406">Unsupported format</response>

        [HttpPost("{version}/{slug}/search/detail/{format:regex(^json|binary$)}", Name="ExtendedSearchDetailed")]
        [HttpPost("{customerCode}/{version}/{slug}/search/detail/{format:regex(^json|binary$)}", Name="ExtendedSearchDetailed-Consolidate")]
        public ActionResult<IEnumerable<Article>> ExtendedSearchDetailed(string slug, string version, string format, 
	        [FromBody] JToken query, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
        {
	        var service = _databaseProductServiceFactory();
            _logger.LogDebug(() => new { slug, version, format, query, isLive }.ToString());
            var ids = service.ExtendedSearchProducts(slug, version, query, isLive);

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            var result = new List<Article>();	
            foreach (var id in ids)
            {
	            result.Add(service.GetProduct(slug, version, id, isLive, includeRelevanceInfo));
            }

            return result;
        }

        /// <summary>
        /// Get product by id. Supports different formats (json, xml, xaml, binary)
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json, xml, binary or xaml</param>
        /// <param name="id">Identifier of the product</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="includeRegionTags">Include or not region tags in the result</param>
        /// <param name="includeRelevanceInfo">Include or not relevance information in the result</param>
        /// <returns>Product</returns>
        /// <response code="406">Unsupported format</response>


        [HttpGet("{version}/{slug}/{format:regex(^json|xml|xaml|binary$)}/{id:int}", Name="GetProduct")]
        [HttpGet("{customerCode}/{version}/{slug}/{format:regex(^json|xml|xaml|binary$)}/{id:int}", Name="GetProduct-Consolidate")]
        public ActionResult<Article> GetProduct(string slug, string version, string format, int id, 
	        bool isLive = false, bool includeRegionTags=false, bool includeRelevanceInfo = false)
		{
			_logger.LogDebug(() => new { slug, version, format, id, isLive }.ToString());

			_accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
		    _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            var product = _databaseProductServiceFactory().GetProduct(slug, version, id, isLive, includeRelevanceInfo);

            if (product == null)
            {
	            return NotFound();
            }

            return product;
		}

        /// <summary>
        /// Get product by list of ids.
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json or binary</param>
        /// <param name="ids">Identifiers of the product separated with ","</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="includeRegionTags">Include or not region tags in the result</param>
        /// <param name="includeRelevanceInfo">Include or not relevance information in the result</param>
        /// <returns>Product list</returns>
        /// <response code="406">Unsupported format</response>

        [HttpGet("{version}/{slug}/list/{format:regex(^json|binary$)}/{ids}", Name="GetProductList")]
        [HttpGet("{customerCode}/{version}/{slug}/list/{format:regex(^json|binary$)}/{ids}", Name="GetProductList-Consolidate")]
        public ActionResult<IEnumerable<Article>> GetProductList(string slug, string version, string format, int[] ids, 
	        bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
        {
            _logger.LogDebug(() => new { slug, version, format, ids, isLive }.ToString());

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

			var result = new List<Article>();
			var service = _databaseProductServiceFactory();
            foreach (var id in ids.Distinct())
            {
                var product = service.GetProduct(slug, version, id, isLive, includeRelevanceInfo);

                if (product != null)
                {
	                result.Add(product);
                }
            }

            return result;
        }

        /// <summary>
        /// Get product relevance for product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="format">json, xml or binary</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns>Product info</returns>
        /// <response code="406">Unsupported format</response>

        [HttpGet("relevance/{format:regex(^json|xml|binary$)}/{id:int}", Name="GetProductRelevance")]
        [HttpGet("{customerCode}/relevance/{format:regex(^json|xml|binary$)}/{id:int}", Name="GetProductRelevance-Consolidate")]
        public ActionResult<Product> GetProductRelevance(int id, string format, bool isLive = false)
        {
	        _logger.LogDebug(() => new { format, id, isLive }.ToString());
            var relevance = _databaseProductServiceFactory().GetRelevance(id, isLive);

            if (relevance == null)
            {
	            return NotFound();
            }

            return new Product(relevance);
        }

        /// <summary>
        /// Get product relevance for products
        /// </summary>
        /// <param name="ids">ProductIds (comma-separated)</param>
        /// <param name="format">json, xml or binary</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns>Product info array</returns>
        /// <response code="406">Unsupported format</response>
 
        [HttpGet("relevance/{format:regex(^json|xml|binary$)}/{ids}", Name="GetProductRelevanceList")]
        [HttpGet("{customerCode}/relevance/{format:regex(^json|xml|binary$)}/{ids}", Name="GetProductRelevanceList-Consolidate")]
        public ActionResult<Product[]> GetProductRelevanceList(int[] ids, string format, bool isLive = false)
        {
	        _logger.LogDebug(() => new { format, ids, isLive }.ToString());
	        var service = _databaseProductServiceFactory();
            return ids.Distinct()
                .Select(id => service.GetRelevance(id, isLive))
                .Where(r => r != null)
                .Select(r => new Product(r))
                .ToArray();
        }


        /// <summary>
        /// Get product built on raw data using tarantool cache
        /// </summary>
        /// <param name="format">json, xml, binary or xaml</param>
        /// <param name="productId">ID of the product</param>
        /// <param name="definitionId">Product definition id</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="includeRegionTags">Include or not region tags in the result</param>
        /// <param name="absent"></param>
        /// <param name="type"></param>
        /// <returns>Product</returns>
        /// <response code="406">Unsupported format</response>
        
        [HttpGet("tarantool/{format:regex(^json|xml|xaml|binary$)}/{productId:int}", Name="TarantoolGet")]        
        [HttpGet("{customerCode}/tarantool/{format:regex(^json|xml|xaml|binary$)}/{productId:int}", Name="TarantoolGet-Consolidate")]        
        public ActionResult<Article> TarantoolGet(string format, int productId, int definitionId, 
	        bool isLive = false, bool includeRegionTags = false, bool absent = false, string type = null)
        {
            _logger.LogDebug(() => new { format, productId, definitionId, isLive, includeRegionTags, absent, type}.ToString());

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            var product = absent ? 
	            _tarantoolProductService.GetAbsentProduct(productId, definitionId, isLive, type) : 
	            _tarantoolProductService.GetProduct(productId, definitionId, isLive);            

            return product;
        }

        /// <summary>
        /// Product publishing using Tarantool 
        /// </summary>
        /// <param name="format">json, xml or binary</param>
        /// <param name="productId">Product Id</param>
        /// <param name="item">Product info</param>
        /// <param name="localize">Localize or not?</param>
        /// <response code="204">Published</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(406)]       
        [HttpPost("tarantool/publish/{format:regex(^json|xml|binary$)}/{productId:int}", Name="TarantoolPublish")]
        [HttpPost("{customerCode}/tarantool/publish/{format:regex(^json|xml|binary$)}/{productId:int}", Name="TarantoolPublish-Consolidate")]
        public ActionResult TarantoolPublish(string format, int productId, [FromBody] ProductItem item, bool localize = true)
        {
	        _logger.LogDebug(() => new { format, productId, item, localize}.ToString());
            _autopublishProcessor.Publish(item, localize);
            return NoContent();
        }

        /// <summary>
        /// Create or update product
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="format">json, xml or xaml</param>
        /// <param name="product">Product in json, xml or xaml format</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <param name="createVersions">Should we create article versions while updating?</param>
        /// <response code="204">Created or updated</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(406)]        
        [HttpPost("{version}/{slug}/{format:regex(^json|xml|xaml$)}/{id:int}", Name="Post")]
        [HttpPost("{customerCode}/{version}/{slug}/{format:regex(^json|xml|xaml$)}/{id:int}", Name="Post-Consolidate")]
        public ActionResult Post(string slug, string version, string format, [FromBody] Article product, bool isLive = false, bool createVersions = false)
		{
			_logger.LogDebug(() => new { slug, version, format, productId = product.Id, productContentId = product.ContentId, isLive, createVersions }.ToString());
			_databaseProductServiceFactory().UpdateProduct(slug, version, product, isLive, createVersions);
			return NoContent();
		}

        /// <summary>
        /// Remove product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format">json or xml</param>
        /// <response code="204">Deleted</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(406)]
        [HttpDelete("{format:regex(^json|xml$)}/{id:int}", Name="Delete")]
        [HttpDelete("{customerCode}/{format:regex(^json|xml$)}/{id:int}", Name="Delete-Consolidate")]
        public ActionResult Delete(int id, string format)
		{
			_logger.LogDebug(() => new { id, format }.ToString());
			_databaseProductServiceFactory().CustomAction("DeleteAction", id);
			return NoContent();			
		}

        /// <summary>
        /// Invoke QP custom action
        /// </summary>
        /// <param name="format">json or xml</param>
        /// <param name="name">The name of custom action</param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        /// <response code="204">Invoked</response>
        /// <response code="406">Unsupported format</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(406)]
        [HttpPost("custom/{format:regex(^json|xml|binary$)}/{name}/{id:int}", Name="CustomAction")]
        [HttpPost("{customerCode}/custom/{format:regex(^json|xml|binary$)}/{name}/{id:int}", Name="CustomAction-Consolidate")]
        public ActionResult CustomAction(string name, int id, string format, [FromBody] Dictionary<string, string> parameters)
		{
			_logger.LogDebug(() => new { name, id, format, parameters }.ToString());
			_databaseProductServiceFactory().CustomAction(name, id, parameters);
			return NoContent();				
		}

        /// <summary>
        /// Get schema of the product type. Supports different formats: json, xml, xaml, jsonDefinition, jsonDefinition2 (extensions represented as backward relations).
        /// </summary>
        /// <param name="slug">Definition identifier (e.g. Tariffs)</param>
        /// <param name="format">json, xml or xaml</param> ///
        /// <param name="version">Definition version (e.g. V1)</param>
        /// <param name="forList"></param>
        /// <param name="includeRegionTags">Include or not region tags in the result</param>
        /// <returns>Schema</returns>
        [HttpGet("{version}/{slug}/schema/{format:regex(^json|xml|xaml|jsonDefinition|jsonDefinition2$)}", Name="Schema")]
        [HttpGet("{customerCode}/{version}/{slug}/schema/{format:regex(^json|xml|xaml|jsonDefinition|jsonDefinition2$)}", Name="Schema-Consolidate")]
        public ActionResult<Content> Schema(string slug, string version, string format, bool forList = false, bool includeRegionTags = false)
		{
			_logger.LogDebug(() => new { slug, version, format, forList, includeRegionTags }.ToString());

			var definition = _databaseProductServiceFactory().GetProductDefinition(slug, version, forList);

            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            return definition.Content;
		}	
	}
}