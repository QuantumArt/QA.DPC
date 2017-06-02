using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Core.API.Filters;
using QA.ProductCatalog.HighloadFront.Core.API.Helpers;
using QA.ProductCatalog.HighloadFront.Elastic;
using ResponseCacheLocation = Microsoft.AspNetCore.Mvc.ResponseCacheLocation;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Produces("application/json")]
    [Route("api/products")]
    [OnlyAuthUsers]
    public class ProductsController : Controller
    {
        private ProductManager Manager { get; }

        public ProductsController(ProductManager manager)
        {
            Manager = manager;
        }


        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[]{"GetByType"})]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("{type}"), Route("{language}/{state}/{type}")]
        public async Task<ActionResult> GetByType(string type, string language = null, string state = null)
        {
            type = type?.TrimStart('@');
            var options = ProductOptionsParser.Parse(Request.Query);
            try
            {
                var stream = await Manager.GetProductsInTypeStream(type, options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return BadRequest($"Error occurred: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }
        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "GetById" })]
        [Route("{language}/{state}/{id:int}"), Route("{id:int}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, VaryByHeader = "fields", Duration = 600)]
        public async Task<ActionResult> GetById(string id, string language = null, string state = null)
        {
            var options = ProductOptionsParser.Parse(Request.Query);

            try
            {
                var elasticResponse = await Manager.FindStreamByIdAsync(id, options, language, state);
                return await GetResponse(elasticResponse.Body, false);
            }
            catch (ElasticsearchClientException ex) when (ex.Response.HttpStatusCode == 404)
            {
                return BadRequest($"Product with id = {id} not found");
            }
            catch (ElasticsearchClientException ex)
            {
                return BadRequest($"Error occurred: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }

        }

        [TypeFilter(typeof(RateLimitAttribute), Arguments = new object[] { "Search" })]
        [Route("{language}/{state}/search"), Route("search"), HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult> Search([FromQuery] string q, string language = null, string state = null)
        {
            var options = ProductOptionsParser.Parse(Request.Query);
            try
            {
                var stream = await Manager.SearchStreamAsync(options, language, state);
                return await GetResponse(stream);
            }
            catch (ElasticsearchClientException ex)
            {
                return BadRequest($"Error occurred: {ex.Response.HttpStatusCode}. Reason: {ex.Message}");
            }
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
