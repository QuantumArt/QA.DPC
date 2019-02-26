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
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.WebApi.App_Start;
using QA.ProductCatalog.WebApi.Filters;
using QA.ProductCatalog.WebApi.Models;


namespace QA.ProductCatalog.WebApi.Controllers
{
    /// <summary>
    /// Product maniputation. Methods support differrent output formats: json, xml, xaml, pdf, binary
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
        /// <param name="slug"></param>
        /// <param name="version">Version of the definition</param>
        /// <param name="isLive"></param>
        /// <param name="startRow"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/{format:media_type_mapping=json}")]
        public ActionResult<Dictionary<string, object>[]> List(string slug, string version, bool isLive = false, long startRow = 0, long pageSize = int.MaxValue)
		{
			_logger.LogDebug(() => new { slug, version, isLive, startRow, pageSize }.ToString());
			return _databaseProductService.GetProductsList(slug, version, isLive, startRow, pageSize);
		}

        /// <summary>
        /// Get list of products by query.
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="query">A part of SQL query (where clause)</param>
        /// <param name="isLive"></param>
        /// <returns></returns>
		[HttpGet]
        [Route("{version}/{slug}/search/{format:media_type_mapping}/{query}")]
        public ActionResult<int[]> Search(string slug, string version, string query, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
			return _databaseProductService.SearchProducts(slug, version, query, isLive);
		}

        /// <summary>
        /// Get detailed list of products by query
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="version"></param>
        /// <param name="query"></param>
        /// <param name="isLive"></param>
        /// <param name="includeRegionTags"></param>
        /// <param name="includeRelevanceInfo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/search/detail/{format:media_type_mapping}/{query}")]
        public ActionResult<IEnumerable<Article>> SearchDetailed(string slug, string version, string query, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
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
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="query">A part of SQL query (where clause)</param>
        /// <param name="isLive"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{version}/{slug}/search/{format:media_type_mapping}")]
        public ActionResult<int[]> ExtendedSearch(string slug, string version,[FromBody] JToken query, bool isLive = false)
        {
            _logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
            return _databaseProductService.ExtendedSearchProducts(slug, version, query, isLive);
        }

        /// <summary>
        /// Get detailed list of products by extended query.
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="query">A part of SQL query (where clause)</param>
        /// <param name="isLive"></param>
        /// <param name="includeRegionTags"></param>
        /// <param name="includeRelevanceInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{version}/{slug}/search/detail/{format:media_type_mapping}")]
        public ActionResult<IEnumerable<Article>> ExtendedSearchDetailed(string slug, string version, [FromBody] JToken query, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
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
        /// Get product by id. Supports differrent formats (json, xml, xaml, binary)
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="id">Indentifier of the product</param>
        /// <param name="isLive"></param>
        /// <param name="includeRegionTags"></param>
        /// <param name="includeRelevanceInfo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/{format:media_type_mapping}/{id:int}")]
        public ActionResult<Article> GetProduct(string slug, string version, int id, bool isLive = false, bool includeRegionTags=false, bool includeRelevanceInfo = false)
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
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="ids">Indentifiers of the product separated with ","</param>
        /// <param name="isLive"></param>
        /// <param name="includeRegionTags"></param>
        /// <param name="includeRelevanceInfo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/list/{format:media_type_mapping}/{ids}")]
        public ActionResult<IEnumerable<Article>> GetProductList(string slug, string version, int[] ids, bool isLive = false, bool includeRegionTags = false, bool includeRelevanceInfo = false)
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
        /// <param name="isLive"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("relevance/{format:media_type_mapping}/{id:int}")]
        public ActionResult<Product> GetProductRelevance(int id, bool isLive = false)
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
        /// <param name="isLive"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("relevance/{format:media_type_mapping}/{ids}")]
        public ActionResult<Product[]> GetProductRelevanceList(int[] ids, bool isLive = false)
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
        /// <param name="productId">ID of the priduct</param>
        /// <param name="definitionId">Producat definition id</param>
        /// <param name="isLive"></param>
        /// <param name="includeRegionTags"></param>
        /// <param name="absent"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("tarantool/{format:media_type_mapping}/{productId:int}")]        
        public ActionResult<Article> TarantoolGet(int productId, int definitionId, bool isLive = false, bool includeRegionTags = false, bool absent = false, string type = null)
        {
            _logger.LogDebug(() => new { productId, definitionId, isLive }.ToString());

            _accessor.HttpContext.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            Article product;

            if (absent)
            {
                product = _tarantoolProductService.GetAbsentProduct(productId, definitionId, isLive, type);
            }
            else
            {
                product = _tarantoolProductService.GetProduct(productId, definitionId, isLive);
            }            

            return product;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="item"></param>
        /// <param name="localize"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("tarantool/publish/{format:media_type_mapping}/{productId:int}")]
        public ActionResult TarantoolPublish(int productId, ProductItem item, bool localize = true)
        {
            _autopublishProcessor.Publish(item, localize);
            return NoContent();
        }

        /// <summary>
        /// Create or update product
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="product"></param>
        /// <param name="isLive"></param>
        /// <param name="createVersions"></param>
        [HttpPost]
        [Route("{version}/{slug}/{format:media_type_mapping}/{id:int}")]
        public ActionResult Post(string slug, string version, Article product, bool isLive = false, bool createVersions = false)
		{
			_logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId, isLive, createVersions }.ToString());
			_databaseProductService.UpdateProduct(slug, version, product, isLive, createVersions);
			return NoContent();
		}

        /// <summary>
        /// Remove product
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete]
        [Route("{format:media_type_mapping}/{id:int}")]
        public ActionResult Delete(int id)
		{
			_logger.LogDebug(() => new { id }.ToString());
			_databaseProductService.CustomAction("DeleteAction", id);
			return NoContent();			
		}

        /// <summary>
        /// Invoke QP custom action
        /// </summary>
        /// <param name="name">The name of custom action</param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
		[HttpPost]
        [Route("custom/{format:media_type_mapping}/{name}/{id:int}")]
        public ActionResult CustomAction(string name, int id, Dictionary<string, string> parameters)
		{
			_logger.LogDebug(() => new { name, id }.ToString());
			_databaseProductService.CustomAction(name, id, parameters);
			return NoContent();				
		}

        /// <summary>
        /// Get schema of the product type. Supports differrent formats: json, xml, xaml, jsonDefinition, jsonDefinition2 (extensions represented as backward relations).
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="forList"></param>
        /// <param name="includeRegionTags"></param>
        /// <returns></returns>
		[HttpGet]
        [Route("{version}/{slug}/schema/{format:media_type_mapping}")]
        public ActionResult<Content> Schema(string slug, string version, bool forList = false, bool includeRegionTags = false)
		{
			_logger.LogDebug(() => new { slug, version, forList }.ToString());

			var definition = _databaseProductService.GetProductDefinition(slug, version, forList);

            _accessor.HttpContext.Items["includeRegionTags"] = includeRegionTags;

            return definition.Content;
		}	
	}
}