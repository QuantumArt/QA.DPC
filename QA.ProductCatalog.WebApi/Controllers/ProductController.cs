using System.Collections.Generic;
using System.Web;
using System.Web.Http;
using QA.Core;
using QA.Core.Logger;
using QA.Core.Models;
using QA.Core.Models.Configuration;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.WebApi.Controllers
{
    /// <summary>
    /// Product maniputation. Methods support differrent output formats: json, xml, xaml, pdf, binary
    /// Supply format via 'format' path segment
    /// </summary>
    public class ProductController : ApiController
	{
		private readonly IProductAPIService _databaseProductService;
        private readonly IProductSimpleAPIService _tarantoolProductService;
        private readonly ILogger _logger;

		public ProductController(IProductAPIService databaseProductService, IProductSimpleAPIService tarantoolProductService, ILogger logger)
		{
			_databaseProductService = databaseProductService;
            _tarantoolProductService = tarantoolProductService;
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
		[AcceptVerbs("GET")]        
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
		[AcceptVerbs("GET")]
		public int[] Search(string slug, string version, string query, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
			return _databaseProductService.SearchProducts(slug, version, query, isLive);
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
        [AcceptVerbs("GET")]
        public Article GetProduct(string slug, string version, int id, bool isLive = false, bool includeRegionTags=false)
		{
			_logger.LogDebug(() => new { slug, version, id, isLive }.ToString());

			HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

		    HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            var product = _databaseProductService.GetProduct(slug, version, id, isLive);

            return product;
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
	    [AcceptVerbs("GET")]
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
        /// Create or update product
        /// </summary>
        /// <param name="slug">Product type as configured in services content</param>
        /// <param name="version">Version of the definition</param>
        /// <param name="product"></param>
        /// <param name="isLive"></param>
        [AcceptVerbs("POST")]
        public void Post(string slug, string version, Article product, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId, isLive }.ToString());
			_databaseProductService.UpdateProduct(slug, version, product, isLive);
		}


        /// <summary>
        /// Remove product
        /// </summary>
        /// <param name="id"></param>
        [AcceptVerbs("DELETE")]
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
		[AcceptVerbs("POST")]
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
		[AcceptVerbs("GET")]
		public Content Schema(string slug, string version, bool forList = false, bool includeRegionTags = false)
		{
			_logger.LogDebug(() => new { slug, version, forList }.ToString());

			var definition = _databaseProductService.GetProductDefinition(slug, version, forList);

            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            return definition.Content;
		}	
	}
}