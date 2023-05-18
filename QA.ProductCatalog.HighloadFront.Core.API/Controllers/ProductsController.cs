using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using NLog;
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
        Route("api/{customerCode}/{version:decimal}"),
        Route("api/{customerCode}/{version:decimal}/{language}/{state}/products"),
        Route("api/{customerCode}/{version:decimal}/{language}/{state}")
    ]
    [OnlyAuthUsers]
    public class ProductsController : BaseProductsController
    {
        private static readonly Regex ParamsToReplace = new Regex(
            @"\|\|(?<name>[\w]+)\:*(?<constraints>(?:[^|]|\|(?!\|))+)?\|\|",
            RegexOptions.Compiled | RegexOptions.Singleline
        );

        private static readonly Regex ConstaintsToProcess = new Regex(
            @"((?<name>\w+)\((?<value>(?:[^\)\:]|(?<!\))\)|(?<!\))\:)+)\))+",
            RegexOptions.Compiled | RegexOptions.Singleline
        );

        private readonly ApiRestrictionOptions _apiRestrictionOptions;

        public ProductsController(
            ProductManager manager,
            ElasticConfiguration configuration,
            SonicElasticStoreOptions elasticOptions,
            ApiRestrictionOptions apiRestrictionOptions,
            IMemoryCache cache)
            : base(manager, configuration, elasticOptions, cache)
        {
            _apiRestrictionOptions = apiRestrictionOptions;
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetByType" })]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}")]
        public async Task<ActionResult> GetByType(ProductsOptionsRoot options, string language = null, string state = null,
            CancellationToken cancellationToken = default)
        {
            CorrectProductOptions(options);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            try
            {
                return await GetSearchActionResult(options, language, state, cancellationToken);
            }
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetByType" })]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}"), HttpPost]
        public async Task<ActionResult> GetByType([FromBody] object json, string type, string language = null, string state = null,
            CancellationToken cancellationToken = default)
        {
            var modelStateResult = HandleBadJsonRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }

            var options = new ProductsOptionsRoot
            {
                Type = type?.TrimStart('@'),
                CacheForSeconds = 0
            }.BuildFromJson<ProductsOptionsRoot>(json, ElasticOptions);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            try
            {
                return await GetSearchActionResult(options, language, state, cancellationToken);
            }
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        [ResponseCache(Location = ResponseCacheLocation.Any, VaryByHeader = "fields", Duration = 600)]
        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetById" })]
        [Route("{id:int}"), HttpPost]
        public async Task<ActionResult> GetById([FromBody] object json, int id, string language = null, string state = null)
        {
            var modelStateResult = HandleBadJsonRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }

            var options = new ProductsOptionsRoot
            {
                Id = id,
                CacheForSeconds = 0
            }.BuildFromJson<ProductsOptionsRoot>(json, ElasticOptions);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

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
        public async Task<ActionResult> GetById(ProductsOptionsRoot options, string language = null, string state = null)
        {
            CorrectProductOptions(options);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

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
        public async Task<ActionResult> Search([FromBody] object json, string language = null, string state = null,
            CancellationToken cancellationToken = default)
        {
            var modelStateResult = HandleBadJsonRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }

            var options = new ProductsOptionsRoot
            {
                CacheForSeconds = 0
            }.BuildFromJson<ProductsOptionsRoot>(json, ElasticOptions);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            try
            {
                return await GetSearchActionResult(options, language, state, cancellationToken);
            }
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "Search" })]
        [Route("search"), HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Search(ProductsOptionsRoot options, string language = null, string state = null,
            CancellationToken cancellationToken = default)
        {
            CorrectProductOptions(options);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            try
            {
                return await GetSearchActionResult(options, language, state, cancellationToken);
            }
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        [TypeFilter(typeof(RateLimitRouteAttribute), Arguments = new object[] { "alias" })]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("query/{alias}")]
        public async Task<ActionResult> Query(string alias, int? id, int? skip, int? take, string language = null, string state = null,
            CancellationToken cancellationToken = default)
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

            var options = new ProductsOptionsRoot()
                .BuildFromJson<ProductsOptionsRoot>(json, ElasticOptions, id, skip, take);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            try
            {
                return await GetSearchActionResult(options, language, state, cancellationToken);
            }
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        [TypeFilter(typeof(RateLimitRouteAttribute), Arguments = new object[] { "alias" })]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("query/{alias}"), HttpPost]
        public async Task<ActionResult> Query([FromBody] object json, string type, string language = null, string state = null,
            CancellationToken cancellationToken = default)
        {
            var modelStateResult = HandleBadJsonRequest();
            if (modelStateResult != null)
            {
                return modelStateResult;
            }

            var options = new ProductsOptionsRoot
            {
                Type = type?.TrimStart('@')
            }.BuildFromJson<ProductsOptionsRoot>(json, ElasticOptions);

            var modelValidationResult = ValidateModel(options);
            if (modelValidationResult != null)
            {
                return modelValidationResult;
            }

            try
            {
                return await GetSearchActionResult(options, language, state, cancellationToken);
            }
            catch (ElasticClientException ex)
            {
                return ElasticBadRequest(ex);
            }
        }

        private void CorrectProductOptions(ProductsOptionsRoot options)
        {
            options.ElasticOptions = ElasticOptions;
            options.ApplyQueryCollection(Request.Query);
            options.ComputeArrays();
        }

        private async Task<ActionResult> GetByIdActionResult(ProductsOptionsRoot options, string language, string state)
        {
            var result = await Manager.FindByIdAsync(options, language, state);
            return Json(result);
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
                var param = string.Join(",", values);
                var replaceValue = GetReplaceValue(param, constraints, defaultValue);
                if (replaceValue == null)
                {
                    string message = !string.IsNullOrEmpty(param) ?
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

            if (!string.IsNullOrEmpty(param) && ValidateParam(param, constraints))
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

        private BadRequestObjectResult HandleBadJsonRequest()
        {
            if (ModelState.IsValid)
            {
                return null;
            }

            var errors = ModelState
                .SelectMany(
                    kvp => kvp.Value.Errors,
                    (kvp, error) => error.Exception?.Message ?? error.ErrorMessage)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();
            if (!errors.Any())
            {
                return null;
            }

            var result = BadRequest(errors);
            var errorsStr = string.Join(",", errors);
            Logger.Error($"Model has errors: {errorsStr}");
            return result;
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

        private BadRequestObjectResult ValidateModel(ProductsOptionsRoot model)
        {
            var isPost = HttpMethods.IsPost(Request.Method);
            var maxExpandDepth = isPost
                ? _apiRestrictionOptions.MaxExpandDepth ?? HighloadConstants.MaxExpandDepthDefault
                : HighloadConstants.MaxExpandDepthForGetRequests;

            if (!IsValidExpandDepth(model, maxExpandDepth))
            {
                return new BadRequestObjectResult($"Max expand depth = {maxExpandDepth} is exceeded");
            }

            return null;
        }

        private bool IsValidExpandDepth(ProductsOptionsRoot model, int maxExpandDepth)
        {
            var initialDepth = 0;
            return IsValidExpandDepthRecursive(model, maxExpandDepth, ref initialDepth);
        }

        private bool IsValidExpandDepthRecursive(ProductsOptionsBase model, int maxExpandDepth, ref int currentDepth)
        {
            if (model.Expand != null)
            {
                currentDepth++;

                if (currentDepth > maxExpandDepth)
                {
                    return false;
                }

                foreach (var childExpandModel in model.Expand)
                {
                    var currentDepthCopy = currentDepth;
                    if (!IsValidExpandDepthRecursive(childExpandModel, maxExpandDepth, ref currentDepthCopy))
                    {
                        return false;
                    }
                }
            }

            return currentDepth <= maxExpandDepth;
        }
    }
}
