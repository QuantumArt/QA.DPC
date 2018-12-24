using Newtonsoft.Json.Linq;
using QA.Core.DPC.QP.Autopublish.Models;
using QA.Core.DPC.QP.Autopublish.Services;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.WebApi.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;

namespace QA.ProductCatalog.WebApi.Controllers
{
    /// <summary>
    /// Product maniputation. Methods support differrent output formats: json, xml, xaml, pdf, binary
    /// Supply format via 'format' path segment
    /// </summary>
    [DynamicRoutePrefix]
    public class ProductController : ApiController
	{
		private readonly IProductAPIService _databaseProductService;
        private readonly IProductSimpleAPIService _tarantoolProductService;
        private readonly IAutopublishProcessor _autopublishProcessor;
        private readonly ILogger _logger;

		public ProductController(IProductAPIService databaseProductService, IProductSimpleAPIService tarantoolProductService, IAutopublishProcessor autopublishProcessor, ILogger logger)
		{
			_databaseProductService = databaseProductService;
            _tarantoolProductService = tarantoolProductService;
            _autopublishProcessor = autopublishProcessor;
            _logger = logger;
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
        public Dictionary<string, object>[] List(string slug, string version, bool isLive = false, long startRow = 0, long pageSize = int.MaxValue)
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
        public int[] Search(string slug, string version, string query, bool isLive = false)
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
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/search/detail/{format:media_type_mapping}/{query}")]
        public IEnumerable<Article> SearchDetailed(string slug, string version, string query, bool isLive = false, bool includeRegionTags = false)
        {
            _logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
            var ids = _databaseProductService.SearchProducts(slug, version, query, isLive);

            HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            foreach (var id in ids)
            {
                yield return _databaseProductService.GetProduct(slug, version, id, isLive);
            }
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
        public int[] ExtendedSearch(string slug, string version,[FromBody] JToken query, bool isLive = false)
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
        /// <returns></returns>
        [HttpPost]
        [Route("{version}/{slug}/search/detail/{format:media_type_mapping}")]
        public IEnumerable<Article> ExtendedSearchDetailed(string slug, string version, [FromBody] JToken query, bool isLive = false, bool includeRegionTags = false)
        {
            _logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
            var ids = _databaseProductService.ExtendedSearchProducts(slug, version, query, isLive);

            HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            foreach (var id in ids)
            {
                yield return _databaseProductService.GetProduct(slug, version, id, isLive);
            }
        }

        /// <summary>
        /// Get product by id. Supports differrent formats (json, xml, xaml, binary)
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="id">Indentifier of the product</param>
        /// <param name="isLive"></param>
        /// <param name="includeRegionTags"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/{format:media_type_mapping}/{id:int}")]
        public Article GetProduct(string slug, string version, int id, bool isLive = false, bool includeRegionTags=false)
		{
			_logger.LogDebug(() => new { slug, version, id, isLive }.ToString());

			HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

		    HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            var product = _databaseProductService.GetProduct(slug, version, id, isLive);

            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
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
        /// <returns></returns>
        [HttpGet]
        [Route("{version}/{slug}/list/{format:media_type_mapping}/{ids}")]
        [ArrayParameter("ids")]
        public IEnumerable<Article> GetProductList(string slug, string version, int[] ids, bool isLive = false, bool includeRegionTags = false)
        {
            _logger.LogDebug(() => new { slug, version, ids, isLive }.ToString());

            HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            foreach (var id in ids.Distinct())
            {
                var product = _databaseProductService.GetProduct(slug, version, id, isLive);

                if (product != null)
                {
                    yield return product;
                }
            }
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
        public Article TarantoolGet(int productId, int definitionId, bool isLive = false, bool includeRegionTags = false, bool absent = false, string type = null)
        {
            _logger.LogDebug(() => new { productId, definitionId, isLive }.ToString());

            HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;
            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

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
        public void TarantoolPublish(int productId, ProductItem item, bool localize = true)
        {
            _autopublishProcessor.Publish(item, localize);
        }

        /// <summary>
        /// Create or update product
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="product"></param>
        /// <param name="isLive"></param>
        [HttpPost]
        [Route("{version}/{slug}/{format:media_type_mapping}/{id:int}")]
        public void Post(string slug, string version, Article product, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId, isLive }.ToString());
			_databaseProductService.UpdateProduct(slug, version, product, isLive);
		}

        /// <summary>
        /// Remove product
        /// </summary>
        /// <param name="id"></param>
        [HttpDelete]
        [Route("{format:media_type_mapping}/{id:int}")]
        public void Delete(int id)
		{
			_logger.LogDebug(() => new { id }.ToString());
			_databaseProductService.CustomAction("DeleteAction", id);
		}

        /// <summary>
        /// Invoke QP custom action
        /// </summary>
        /// <param name="name">The name of custom action</param>
        /// <param name="id"></param>
        /// <param name="parameters"></param>
		[HttpPost]
        [Route("custom/{format:media_type_mapping}/{name}/{id:int}")]
        public void CustomAction(string name, int id, Dictionary<string, string> parameters)
		{
			_logger.LogDebug(() => new { name, id }.ToString());
			_databaseProductService.CustomAction(name, id, parameters);
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
        public Content Schema(string slug, string version, bool forList = false, bool includeRegionTags = false)
		{
			_logger.LogDebug(() => new { slug, version, forList }.ToString());

			var definition = _databaseProductService.GetProductDefinition(slug, version, forList);

            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            return definition.Content;
		}	
	}
}