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
using QA.ProductCatalog.WebApi.Filters;
using QA.ProductCatalog.WebApi.Models;


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
		private readonly IProductAPIService _databaseProductService;
        private readonly IProductSimpleAPIService _tarantoolProductService;
        private readonly IAutopublishProcessor _autopublishProcessor;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _accessor;

		public ProductController(
			IProductAPIService databaseProductService, 
			IProductSimpleAPIService tarantoolProductService, 
			IAutopublishProcessor autopublishProcessor, 
			ILogger logger,
			IHttpContextAccessor accessor
			)
		{
			_databaseProductService = databaseProductService;
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
        [HttpGet]
        [Route("{version}/{slug}/{format:regex(^json|xml|binary$)}")]
        public ActionResult<Dictionary<string, object>[]> List(string version, string slug, string format, 
	        bool isLive = false, long startRow = 0, long pageSize = 10)
		{
			_logger.LogDebug(() => new { version, slug, isLive, startRow, pageSize }.ToString());
			var result = _databaseProductService.GetProductsList(slug, version, isLive, startRow, pageSize);
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
		[HttpGet]
        [Route("{version}/{slug}/search/{format:regex(^json|xml|binary$)}/{query}")]
        public ActionResult<int[]> Search(string slug, string version, string format, string query, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
			return _databaseProductService.SearchProducts(slug, version, query, isLive);
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
        [HttpGet]
        [Route("{version}/{slug}/search/detail/{format:regex(^json|binary$)}/{query}")]
        public ActionResult<IEnumerable<Article>> SearchDetailed(string slug, string version, string format, 
	        string query, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
        {
            _logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
            var ids = _databaseProductService.SearchProducts(slug, version, query, isLive);

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

			var result = new List<Article>();				
            foreach (var id in ids)
            {
	            result.Add(_databaseProductService.GetProduct(slug, version, id, isLive, includeRelevanceInfo));
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
        [HttpPost]
        [Route("{version}/{slug}/search/{format:regex(^json|xml|binary$)}")]
        public ActionResult<int[]> ExtendedSearch(string slug, string version, string format, [FromBody] JToken query, bool isLive = false)
        {
            _logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
            return _databaseProductService.ExtendedSearchProducts(slug, version, query, isLive);
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
        [HttpPost]
        [Route("{version}/{slug}/search/detail/{format:regex(^json|binary$)}")]
        public ActionResult<IEnumerable<Article>> ExtendedSearchDetailed(string slug, string version, string format, 
	        [FromBody] JToken query, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
        {
            _logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
            var ids = _databaseProductService.ExtendedSearchProducts(slug, version, query, isLive);

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            var result = new List<Article>();	
            foreach (var id in ids)
            {
	            result.Add(_databaseProductService.GetProduct(slug, version, id, isLive, includeRelevanceInfo));
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
        [HttpGet]
        [Route("{version}/{slug}/{format:regex(^json|xml|xaml|binary$)}/{id:int}")]
        public ActionResult<Article> GetProduct(string slug, string version, string format, int id, 
	        bool isLive = false, bool includeRegionTags=false, bool includeRelevanceInfo = false)
		{
			_logger.LogDebug(() => new { slug, version, id, isLive }.ToString());

			_accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
		    _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            var product = _databaseProductService.GetProduct(slug, version, id, isLive, includeRelevanceInfo);

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
        [HttpGet]
        [Route("{version}/{slug}/list/{format:regex(^json|binary$)}/{ids}")]
        public ActionResult<IEnumerable<Article>> GetProductList(string slug, string version, string format, int[] ids, 
	        bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
        {
            _logger.LogDebug(() => new { slug, version, ids, isLive }.ToString());

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

			var result = new List<Article>();
            foreach (var id in ids.Distinct())
            {
                var product = _databaseProductService.GetProduct(slug, version, id, isLive, includeRelevanceInfo);

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
        /// <param name="id"></param>
        /// <param name="format">json, xml or binary</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns></returns>
        [HttpGet]
        [Route("relevance/{format:regex(^json|xml|binary$)}/{id:int}")]
        public ActionResult<Product> GetProductRelevance(int id, string format, bool isLive = false)
        {
            var relevance = _databaseProductService.GetRelevance(id, isLive);

            if (relevance == null)
            {
	            return NotFound();
            }

            return new Product(relevance);
        }

        /// <summary>
        /// Get product relevance for products
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="format">json, xml or binary</param>
        /// <param name="isLive">Should we read live or stage data?</param>
        /// <returns></returns>
        [HttpGet]
        [Route("relevance/{format:regex(^json|xml|binary$)}/{ids}")]
        public ActionResult<Product[]> GetProductRelevanceList(int[] ids, string format, bool isLive = false)
        {
            return ids.Distinct()
                .Select(id => _databaseProductService.GetRelevance(id, isLive))
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
        /// <returns></returns>
        [HttpGet]
        [Route("tarantool/{format:regex(^json|xml|xaml|binary$)}/{productId:int}")]        
        public ActionResult<Article> TarantoolGet(string format, int productId, int definitionId, 
	        bool isLive = false, bool includeRegionTags = false, bool absent = false, string type = null)
        {
            _logger.LogDebug(() => new { productId, definitionId, isLive }.ToString());

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            var product = absent ? 
	            _tarantoolProductService.GetAbsentProduct(productId, definitionId, isLive, type) : 
	            _tarantoolProductService.GetProduct(productId, definitionId, isLive);            

            return product;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="format">json or xml</param>
        /// <param name="productId"></param>
        /// <param name="item"></param>
        /// <param name="localize"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("tarantool/publish/{format:regex(^json|xml$)}/{productId:int}")]
        public ActionResult TarantoolPublish(string format, int productId, [FromBody] ProductItem item, bool localize = true)
        {
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
        [HttpPost]
        [Route("{version}/{slug}/{format:regex(^json|xml|xaml$)}/{id:int}")]
        public ActionResult Post(string slug, string version, string format, [FromBody] Article product, bool isLive = false, bool createVersions = false)
		{
			_logger.LogDebug(() => new { slug, version, format, productId = product.Id, productContentId = product.ContentId, isLive, createVersions }.ToString());
			_databaseProductService.UpdateProduct(slug, version, product, isLive, createVersions);
			return NoContent();
		}

        /// <summary>
        /// Remove product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="format">json or xml</param>
        [HttpDelete]
        [Route("{format:regex(^json|xml$)}/{id:int}")]
        public ActionResult Delete(int id, string format)
		{
			_logger.LogDebug(() => new { id }.ToString());
			_databaseProductService.CustomAction("DeleteAction", id);
			return NoContent();			
		}

        /// <summary>
        /// Invoke QP custom action
        /// </summary>
        /// <param name="format">json or xml</param>
        /// <param name="name">The name of custom action</param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
        [HttpPost]
        [Route("custom/{format:regex(^json|xml$)}/{name}/{id:int}")]
        public ActionResult CustomAction(string name, int id, string format, Dictionary<string, string> parameters)
		{
			_logger.LogDebug(() => new { name, id }.ToString());
			_databaseProductService.CustomAction(name, id, parameters);
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
		[HttpGet]
        [Route("{version}/{slug}/schema/{format:regex(^json|xml|xaml|jsonDefinition|jsonDefinition2$)}")]
        public ActionResult<Content> Schema(string slug, string version, string format, bool forList = false, bool includeRegionTags = false)
		{
			_logger.LogDebug(() => new { slug, version, forList }.ToString());

			var definition = _databaseProductService.GetProductDefinition(slug, version, forList);

            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            return definition.Content;
		}	
	}
}