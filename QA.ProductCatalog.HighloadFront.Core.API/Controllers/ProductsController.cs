using System;
using System.IO;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.Extensions;
using QA.Core.Logger;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Options;
using ResponseCacheLocation = Microsoft.AspNetCore.Mvc.ResponseCacheLocation;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Produces("application/json")]
    [
        Route("api/products"),
        Route("api/{version:decimal}/products"),
        Route("api/{version:decimal}/{language}/{state}/products"),
        Route("api/{customerCode}/products"),
        Route("api/{customerCode}/{language}/{state}/products"),
        Route("api/{customerCode}/{version:decimal}/products"),
        Route("api/{customerCode}/{version:decimal}/{language}/{state}/products"),
    ]
    [OnlyAuthUsers]
    public class ProductsController : Controller
    {
        private ProductManager Manager { get; }

        private readonly SonicElasticStoreOptions _options;

        private readonly ILogger _logger;

        public ProductsController(ProductManager manager, IOptions<SonicElasticStoreOptions> options, ILogger logger)
        {
            Manager = manager;
            _options = options.Value;
            _logger = logger;
        }


        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[]{"GetByType"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}")]
        public async Task<ActionResult> GetByType(ProductsOptions options, string language = null, string state = null)
        {
            try
            {
                var stream = await Manager.GetProductsInTypeStream(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                LogElasticException(ex);
                return BadRequest($"Elastic search error occurred:: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }
        }
        
        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[]{"GetByType"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}"), HttpPost]
        public async Task<ActionResult> GetByType([FromBody]ProductsOptions options, string type, string language = null, string state = null)
        {
            options.Type = type?.TrimStart('@');
            try
            {
                var stream = await Manager.GetProductsInTypeStream(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                LogElasticException(ex);
                return BadRequest($"Elastic search error occurred:: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetById" })]
        [Route("{id:int}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, VaryByHeader = "fields", Duration = 600)]
        public async Task<ActionResult> GetById(string id, string language = null, string state = null)
        {
            var options = ProductOptionsParser.Parse(Request.Query, _options);

            try
            {
                var elasticResponse = await Manager.FindStreamByIdAsync(id, options, language, state);
                return await GetResponse(elasticResponse.Body, false);
            }
            catch (ElasticsearchClientException ex)
            {
                if (ex.Response.HttpStatusCode == 404)
                {
                    _logger.ErrorException($"Product with id = {id} not found", ex);
                    return BadRequest($"Product with id = {id} not found");                  
                }
                LogElasticException(ex);
                return BadRequest($"Elastic search error occurred:: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Unexpected error occured", ex);
                return BadRequest($"Unexpected error occurred. Reason: {ex.Message}");
            }

        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "Search" })]
        [Route("search"), HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Search([FromQuery] string q, string language = null, string state = null)
        {
            var options = ProductOptionsParser.Parse(Request.Query, _options);
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                LogElasticException(ex);
                return BadRequest($"Elastic search error occurred:: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }
        }

        private async Task<ActionResult> GetResponse(Stream stream, bool filter = true)
        {
            if (!filter) return new JsonStreamResult(stream);
            var ms = new MemoryStream();
            await JsonFragmentExtractor.ExtractJsonFragment("_source", stream, ms, 4);
            return new JsonStreamResult(ms);

        }

        private void LogElasticException(ElasticsearchClientException ex)
        {
            _logger.ErrorException($"Elastic search error occurred. Debug Info: {ex.DebugInformation}", ex);
        }
    }
}
