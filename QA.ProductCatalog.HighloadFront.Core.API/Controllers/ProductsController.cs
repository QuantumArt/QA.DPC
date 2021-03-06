﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Options;
using ResponseCacheLocation = Microsoft.AspNetCore.Mvc.ResponseCacheLocation;
using NLog;

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
        Route("api/{customerCode}/{version:decimal}"),
        Route("api/{customerCode}/{version:decimal}/{language}/{state}/products"),
        Route("api/{customerCode}/{version:decimal}/{language}/{state}"),

    ]
    [OnlyAuthUsers]
    public class ProductsController : BaseController
    {
        private static readonly Regex ParamsToReplace = new Regex(
            @"\|\|(?<name>[\w]+)\:*(?<constraints>(?:[^|]|\|(?!\|))+)?\|\|", 
            RegexOptions.Compiled | RegexOptions.Singleline
        );
        
        private static readonly Regex ConstaintsToProcess = new Regex(
            @"((?<name>\w+)\((?<value>(?:[^\)\:]|(?<!\))\)|(?<!\))\:)+)\))+", 
            RegexOptions.Compiled | RegexOptions.Singleline
        );

        private readonly SonicElasticStoreOptions _options;

        private readonly IMemoryCache _cache;
        

        public ProductsController(
            ProductManager manager, 
            SonicElasticStoreOptions options, 
            ElasticConfiguration configuration, 
            IMemoryCache cache
        ) : base(manager, configuration)
        {
            _options = options;
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
            catch (ElasticClientException ex)
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
            catch (ElasticClientException ex)
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
            catch (ElasticClientException ex)
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
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
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
            catch (ElasticClientException ex)
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
            catch (ElasticClientException ex)
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
            catch (ElasticClientException ex)
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
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        private async Task<ActionResult> GetByIdActionResult(ProductsOptions options, string language, string state)
        {
            var result = await Manager.FindByIdAsync(options, language, state);
            return Content(result, "application/json");
        }
        
        private async Task<ActionResult> GetSearchActionResult(ProductsOptions options, string language, string state)
        {
            bool readData = true;
            ActionResult result = null; 
            var key = options.GetKey();
            var useCaching = options.CacheForSeconds > 0 && key != null;
            if (useCaching && _cache.TryGetValue(key, out var value))
            {
                result = (ActionResult)value;
                readData = false;
            }

            if (readData)
            {
                var searchResult = await Manager.SearchAsync(options, language, state);
                
                if (options.DataFilters.Any())
                {
                    result = PostProcess(searchResult, options.DataFilters);
                }
                else
                {
                    result = await GetResponse(searchResult);
                }

                if (useCaching)
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
                    var jArrays = hit.SelectTokens(df.Key).OfType<JArray>().ToArray();
                    foreach (var jArray in jArrays)
                    {
                        var relevantTokens = jArray.SelectTokens(df.Value).ToArray();
                        jArray.Clear();
                        foreach (var rToken in relevantTokens)
                        {
                            jArray.Add(rToken);
                        }                    
                    }
                }
            }
            
            return Json(new JArray(hits.Select(n => (object)n)));
        }

        private JObject GetQueryJson(string alias)
        {
            var json = Configuration.GetJsonByAlias(alias);
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
                    Logger.Error($"Model has errors: {errorstr}");                    
                }
            }
            
            return result; 
        }

        private BadRequestObjectResult ElasticBadRequest(ElasticClientException ex, int id = 0)
        {

            LogException(ex, "Elastic Search error occurred: ");
            return BadRequest($"Elastic search error occurred: Reason: {ex.Message}");
        }
        
        private BadRequestObjectResult ParseBadRequest(Exception ex)
        {
            if (ex is ParseJsonException pex)
            {
                LogException(pex, $"Parsing JSON query error for JSON: {pex.Json}");            
            }
            else
            {
                LogException(ex, $"Parsing JSON query error");                 
            }
            
            return BadRequest($"Parsing JSON query error: {ex.Message}");
        }
        
        private BadRequestObjectResult UnexpectedBadRequest(Exception ex)
        {
            LogException(ex, "Unexpected error occured");
            return BadRequest($"Unexpected error occurred. Reason: {ex.Message}");
        }


        private async Task<ActionResult> GetResponse(string text, bool filter = true)
        {
            var result = text;
            if (filter)
            {
                var sb = new StringBuilder();
                await JsonFragmentExtractor.ExtractJsonFragment("_source", text, sb, 4);
                result = sb.ToString();
            }
            return Content(result, "application/json");
        }

    }
}
