using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QA.Core;
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
        private static readonly Regex ParamsToReplace = new Regex(
            @"\|\|(?<name>[\w]+)\:*(?<constraints>(?:[^|]|\|(?!\|))+)?\|\|", 
            RegexOptions.Compiled | RegexOptions.Singleline
        );
        
        private static readonly Regex ConstaintsToProcess = new Regex(
            @"((?<name>\w+)\((?<value>(?:[^\)\:]|(?<!\))\)|(?<!\))\:)+)\))+", 
            RegexOptions.Compiled | RegexOptions.Singleline
        );

        private readonly ProductManager _manager;

        private readonly SonicElasticStoreOptions _options;

        private readonly ILogger _logger;

        private readonly IElasticConfiguration _configuration;

        private readonly IMemoryCache _cache;
        

        public ProductsController(ProductManager manager, SonicElasticStoreOptions options, ILogger logger, IElasticConfiguration configuration, IMemoryCache cache)
        {
            _manager = manager;
            _options = options;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }


        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[]{"GetByType"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}")]
        public async Task<ActionResult> GetByType(ProductsOptions options, string language = null, string state = null)
        {
            CorrectProductOptions(options);
            try
            {
                return await GetSearchActionResult(options, language, state);
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
            var modelStateResult = ModelStateBadRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }
            
            var options = new ProductsOptions(json, _options)
            {
                Type = type?.TrimStart('@'),
                CacheForSeconds = 0
            };
            try
            {
                return await GetSearchActionResult(options, language, state);
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
            var modelStateResult = ModelStateBadRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }
            
            var options = new ProductsOptions(json, _options)
            {
                Id = id,
                CacheForSeconds = 0                
            };
            try
            {
                return await GetByIdActionResult(options, language, state);
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
            CorrectProductOptions(options);
            try
            {
                return await GetByIdActionResult(options, language, state);
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

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "Search" })]
        [Route("search"), HttpPost]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Search([FromBody]object json, string language = null, string state = null)
        {
            var modelStateResult = ModelStateBadRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }
            
            var options = new ProductsOptions(json, _options) {CacheForSeconds = 0};
            try
            {
                return await GetSearchActionResult(options, language, state);
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
            CorrectProductOptions(options);
            try
            {
                return await GetSearchActionResult(options, language, state);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        private void CorrectProductOptions(ProductsOptions options)
        {
            options.ElasticOptions = _options;
            options.ApplyQueryCollection(Request.Query);
            options.ComputeArrays();
        }
        
        
        [TypeFilter(typeof(RateLimitRouteAttribute), Arguments = new object[]{"alias"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("query/{alias}")]
        public async Task<ActionResult> Query(string alias, int? id, int? skip, int? take, string language = null, string state = null)
        {
            JObject json = null;
            try
            {
                json = GetQueryJson(alias);
            }
            catch (Exception ex)
            {
                return ParseBadRequest(ex);
            }
            
            var options = new ProductsOptions(json, _options, id, skip, take);
            
            try
            {
                return await GetSearchActionResult(options, language, state);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }
        
        [TypeFilter(typeof(RateLimitRouteAttribute), Arguments = new object[]{"alias"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("query/{alias}"), HttpPost]
        public async Task<ActionResult> Query([FromBody]object json, string type, string language = null, string state = null)
        {
            var modelStateResult = ModelStateBadRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }
            
            var options = new ProductsOptions(json, _options) {Type = type?.TrimStart('@')};
            
            try
            {
                return await GetSearchActionResult(options, language, state);
            }
            catch (ElasticsearchClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        private async Task<ActionResult> GetByIdActionResult(ProductsOptions options, string language, string state)
        {
            var stream = await _manager.FindStreamByIdAsync(options, language, state);
            return await GetResponse(stream.Body, false);
        }
        
        private async Task<ActionResult> GetSearchActionResult(ProductsOptions options, string language, string state)
        {
            bool readData = true;
            ActionResult result = null; 
            var key = options.GetKey();
            if (options.CacheForSeconds > 0 && key != null && _cache.TryGetValue(key, out var value))
            {
                result = (ActionResult)value;
                readData = false;
            }

            if (readData)
            {
                if (options.DataFilters.Any())
                {
                    var searchResult = await _manager.SearchAsync(options, language, state);
                    result = PostProcess(searchResult, options.DataFilters);
                }
                else
                {
                    var stream = await _manager.SearchStreamAsync(options, language, state);
                    result = await GetResponse(stream);
                }

                if (options.CacheForSeconds > 0 && key != null)
                {
                    _cache.Set(key, result, GetCacheOptions((int)options.CacheForSeconds));
                }
            }

            return result;
        }

        private MemoryCacheEntryOptions GetCacheOptions(int value)
        {
            var options = new MemoryCacheEntryOptions();
            options.SetAbsoluteExpiration(TimeSpan.FromSeconds(value));
            return options;
        }

        private ActionResult PostProcess(string input, Dictionary<string, string> optionsDataFilters)
        {
            const string sourceQuery = "hits.hits.[?(@._source)]._source";
            var hits = JObject.Parse(input).SelectTokens(sourceQuery).ToArray();
            foreach (var df in optionsDataFilters)
            {
                foreach (var hit in hits)
                {
                    var token = hit.SelectToken(df.Key);
                    if (token == null || !(token is JArray jArray)) continue;
                    var relevantTokens = jArray.SelectTokens(df.Value).ToArray();
                    jArray.Clear();
                    foreach (var rToken in relevantTokens)
                    {
                        jArray.Add(rToken);
                    }
                }
            }
            
            return Json(new JArray(hits.Select(n => (object)n)));
        }

        private JObject GetQueryJson(string alias)
        {
            var json = _configuration.GetJsonByAlias(alias);
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
            var matches = ParamsToReplace.Matches(json);
            foreach (Match m in matches)
            {
                var key = m.Groups[0].Value;
                if (result.ContainsKey(key)) continue;
                
                var name = m.Groups["name"].Value;
                var constr = m.Groups["constraints"].Value;
                var constraints = FillConstraints(ConstaintsToProcess.Matches(constr), out var defaultValue);
                string[] values = requestQuery[name];
                var param = String.Join(",", values);
                var replaceValue = GetReplaceValue(param, constraints, defaultValue);
                if (replaceValue == null)
                {
                    string message = !String.IsNullOrEmpty(param) ? 
                        $"Validation of parameter '{name}' failed and no default value has been provided" : 
                        $"Neither parameter '{name}' nor default value has been provided";
                    throw new ParseJsonException(message) { Json = json };
                }

                result.Add(key, replaceValue);

            }

            return result;
        }

        private static Dictionary<string, string> FillConstraints(MatchCollection constraintsMatches, out string defaultValue)
        {
            var constraints = new Dictionary<string, string>();
            defaultValue = null;
            foreach (Match cm in constraintsMatches)
            {
                var cname = cm.Groups["name"].Value;
                var cvalue = cm.Groups["value"].Value;
                if (cname == "default")
                {
                    defaultValue = cvalue;
                }
                else
                {
                    constraints[cname] = cvalue;
                }
            }

            return constraints;
        }

        private string GetReplaceValue(string param, Dictionary<string, string> constraints, string defaultValue)
        {
            string replaceValue = null;
            
            if (!String.IsNullOrEmpty(param) && ValidateParam(param, constraints))
            {
                replaceValue = param;
            }
            else if (defaultValue != null)
            {
                replaceValue = defaultValue;
            }

            return replaceValue;
        }

        private static bool AllInt(string s)
        {
            return s.Split(',').All(n => int.TryParse(n.Trim(), out _));
        }
        
        private static bool AllDecimal(string s)
        {
            return s.Split(',').All(n => decimal.TryParse(n.Trim(), out _));
        }


        private bool ValidateParam(string value, Dictionary<string, string> constraints)
        {
            var result = true;
            foreach (var key in constraints.Keys)
            {
                var constraint = constraints[key];
                switch (key)
                {
                    case "type":
                        switch (constraint)
                        {
                            case "int":
                                result &= int.TryParse(value, out _);
                                break;
                            case "decimal":
                                result &= decimal.TryParse(value, out _);
                                break;
                            case "list_int":
                                result &= AllInt(value); 
                                break;
                            case "list_decimal":
                                result &= AllDecimal(value); 
                                break;                                
                        }
                        break;
                    
                    case "regex":
                        Regex re = new Regex(constraint.Replace(@"\\", @"\"));
                        result &= re.IsMatch(value);
                        break;
                }

                if (!result) break;

            }

            return result;

        }

        private BadRequestObjectResult ModelStateBadRequest()
        {
            BadRequestObjectResult result = null;

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .SelectMany(x => x.Value.Errors, (y, z) => z.Exception?.Message).ToArray();
                if (errors.Any(n => !String.IsNullOrEmpty(n)))
                {
                    result = BadRequest(errors);
                    var errorstr = string.Join(",", errors);
                    _logger.Error($"Model has errors: {errorstr}");                    
                }
            }
            
            return result; 
        }

        private BadRequestObjectResult ElasticBadRequest(ElasticsearchClientException ex, int id = 0)
        {
            if (ex.Response.HttpStatusCode == 404 && id != 0)
            {
                _logger.ErrorException($"Product with id = {id} not found", ex);
                return BadRequest($"Product with id = {id} not found");
            }

            _logger.ErrorException($"Elastic search error occurred. Debug Info: {ex.DebugInformation}", ex);            
            return BadRequest($"Elastic search error occurred: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");

        }
        
        private BadRequestObjectResult ParseBadRequest(Exception ex)
        {
            if (ex is ParseJsonException pex)
            {
                _logger.ErrorException($"Parsing JSON query error for JSON: {pex.Json}", pex);            
            }
            else
            {
                _logger.ErrorException($"Parsing JSON query error", ex);                 
            }
            
            return BadRequest($"Parsing JSON query error: {ex.Message}");
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
