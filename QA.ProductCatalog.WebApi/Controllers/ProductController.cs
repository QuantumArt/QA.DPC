using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using QA.Core.Models;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using QA.Core;
using QA.Core.DPC.Loader;
using System.Net.Http;
using QA.Core.Models.Configuration;

namespace QA.ProductCatalog.WebApi.Controllers
{
	public class ProductController : ApiController
	{
		private readonly IProductAPIService _productService;
		private readonly ILogger _logger;

		public ProductController(IProductAPIService productService, ILogger logger)
		{
			_productService = productService;
			_logger = logger;
		}

		[AcceptVerbs("GET")]
		public Dictionary<string, object>[] List(string slug, string version, bool isLive = false, long startRow = 0, long pageSize = int.MaxValue)
		{
			_logger.LogDebug(() => new { slug, version, isLive, startRow, pageSize }.ToString());
			return _productService.GetProductsList(slug, version, isLive, startRow, pageSize);
		}

		[AcceptVerbs("GET")]
		public int[] Search(string slug, string version, string query, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, query, isLive }.ToString());
			return _productService.SearchProducts(slug, version, query, isLive);
		}

		public Article Get(string slug, string version, int id, bool isLive = false, bool includeRegionTags=false)
		{
			_logger.LogDebug(() => new { slug, version, id, isLive }.ToString());

			HttpContext.Current.Items["ArticleFilter"] = isLive ? ArticleFilter.LiveFilter : ArticleFilter.DefaultFilter;

		    HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            var product = _productService.GetProduct(slug, version, id, isLive);

            return product;
		}

		public void Post(string slug, string version, Article product, bool isLive = false)
		{
			_logger.LogDebug(() => new { slug, version, productId = product.Id, productContentId = product.ContentId, isLive }.ToString());
			_productService.UpdateProduct(slug, version, product, isLive);
		}

		public void Delete(int id)
		{
			_logger.LogDebug(() => new { id }.ToString());
			_productService.CustomAction("DeleteAction", id);
		}

		[AcceptVerbs("POST")]
		public void CustomAction(string name, int id, Dictionary<string, string> parameters)
		{
			_logger.LogDebug(() => new { name, id }.ToString());
			_productService.CustomAction(name, id, parameters);
		}

		[AcceptVerbs("GET")]
		public Content Schema(string slug, string version, bool forList = false, bool includeRegionTags = false)
		{
			_logger.LogDebug(() => new { slug, version, forList }.ToString());

			var definition = _productService.GetProductDefinition(slug, version, forList);

            HttpContext.Current.Items["includeRegionTags"] = includeRegionTags;

            return definition.Content;
		}	
	}
}