using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core.Extensions;
using QA.Core.Logger;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Options;
using Quantumart.QP8.Constants;
using ResponseCacheLocation = Microsoft.AspNetCore.Mvc.ResponseCacheLocation;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Produces("application/json")]
    [
        Route("api/products"),
        
        Route("api/{version:decimal}/products"),
        Route("api/{version:decimal}"),
        
        Route("api/{version:decimal}/{language}/{state}/products"),
        Route("api/{version:decimal}/{language}/{state}"),
        
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

        private readonly IElasticConfiguration _configuration;
        
        private static Regex _paramsToReplace = new Regex(@"\|\|(?<name>[\w]+)(?<type>\:[\w]+)?\|\|", RegexOptions.Compiled | RegexOptions.Multiline);

        public ProductsController(ProductManager manager, IOptions<SonicElasticStoreOptions> options, ILogger logger, IElasticConfiguration configuration)
        {
            Manager = manager;
            _options = options.Value;
            _logger = logger;
            _configuration = configuration;
        }


        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[]{"GetByType"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}")]
        public async Task<ActionResult> GetByType(ProductsOptions options, string language = null, string state = null)
        {
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }
        
        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[]{"GetByType"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}"), HttpPost]
        public async Task<ActionResult> GetByType([FromBody]object json, string type, string language = null, string state = null)
        {
            var options = new ProductsOptions(json, _options) {Type = type?.TrimStart('@')};
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }
        
        
        [ResponseCache(Location = ResponseCacheLocation.Any, VaryByHeader = "fields", Duration = 600)]
        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetById" })]
        [Route("{id:int}"), HttpPost]
        public async Task<ActionResult> GetById([FromBody]object json, int id, string language = null, string state = null)
        {
            var options = new ProductsOptions(json, _options) {Id = id};
            try
            {
                var stream = await Manager.FindStreamByIdAsync(options, language, state);
                return await GetResponse(stream.Body, false);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex, id);
            }
            catch (Exception ex)
            {
                return UnexpectedBadRequest(ex);
            }            
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetById" })]
        [Route("{id:int}"), HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Any, VaryByHeader = "fields", Duration = 600)]
        public async Task<ActionResult> GetById(ProductsOptions options, string language = null, string state = null)
        {
            try
            {
                var stream = await Manager.FindStreamByIdAsync(options, language, state);
                return await GetResponse(stream.Body, false);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex, options.Id);

            }
            catch (Exception ex)
            {
                return UnexpectedBadRequest(ex);
            }

        }

        private ActionResult NotFoundBadRequest(ProductsOptions options, ElasticsearchClientException ex)
        {
            _logger.ErrorException($"Product with id = {options.Id} not found", ex);
            return BadRequest($"Product with id = {options.Id} not found");
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "Search" })]
        [Route("search"), HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Search([FromBody]object json, string language = null, string state = null)
        {
            var options = new ProductsOptions(json, _options);
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }
        
        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "Search" })]
        [Route("search"), HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Search(ProductsOptions options, string language = null, string state = null)
        {
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }
        
        
        [TypeFilter(typeof(RateLimitRouteAttribute), Arguments = new object[]{"alias"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("query/{alias}")]
        public async Task<ActionResult> Query(string alias, int? id, string language = null, string state = null)
        {
            var json = GetQueryJson(alias);
            var options = new ProductsOptions(json, _options, id ?? 0);
            
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        private JObject GetQueryJson(string alias)
        {
            string json = _configuration.GetJsonByAlias(alias);
            var dict = GetParametersToReplace(json, HttpContext.Request.Query);
            foreach (var kv in dict)
            {
                json = json.Replace(kv.Key, kv.Value);
            }

            return JObject.Parse(json);
        }

        private Dictionary<string, string> GetParametersToReplace(string json, IQueryCollection requestQuery)
        {

            var result = new Dictionary<string, string>();
            var matches = _paramsToReplace.Matches(json);
            foreach (Match m in matches)
            {
                var key = m.Groups[0].Value;
                var name = m.Groups["name"].Value;
                var type = m.Groups["type"].Value;
                var param = (string)requestQuery[name];

                if (ValidateParam(param, type))
                {
                    result.Add(key, param);
                }
            }

            return result;
        }

        private bool ValidateParam(string name, string type)
        {
            return true;
        }

        [TypeFilter(typeof(RateLimitRouteAttribute), Arguments = new object[]{"alias"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("query/{alias}"), HttpPost]
        public async Task<ActionResult> Query([FromBody]object json, string type, string language = null, string state = null)
        {
            var options = new ProductsOptions(json, _options) {Type = type?.TrimStart('@')};
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        private BadRequestObjectResult ElasticBadRequest(ElasticsearchClientException ex, int id = 0)
        {
            if (ex.Response.HttpStatusCode == 404 && id != 0)
            {
                _logger.ErrorException($"Product with id = {id} not found", ex);
                return BadRequest($"Product with id = {id} not found");
            }
            else
            {
                _logger.ErrorException($"Elastic search error occurred. Debug Info: {ex.DebugInformation}", ex);            
                return BadRequest($"Elastic search error occurred:: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");                
            }

        }
        
        private BadRequestObjectResult UnexpectedBadRequest(Exception ex)
        {
            _logger.ErrorException("Unexpected error occured", ex);
            return BadRequest($"Unexpected error occurred. Reason: {ex.Message}");
        }


        private async Task<ActionResult> GetResponse(Stream stream, bool filter = true)
        {
            if (!filter) return new JsonStreamResult(stream);
            var ms = new MemoryStream();
            await JsonFragmentExtractor.ExtractJsonFragment("_source", stream, ms, 4);
            return new JsonStreamResult(ms);

        }

    }
}
