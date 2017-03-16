using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Elasticsearch.Net;
using QA.ProductCatalog.HighloadFront.App_Core;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Filters;

namespace QA.ProductCatalog.HighloadFront.Controllers
{
    [RoutePrefix("api/1.0")]
    [OnlyAuthUsers]
    public class ProductsController : ApiController
    {
        private ProductManager Manager { get; }
        private Core.ILogger Logger { get; }

        public ProductsController(ProductManager manager, Core.ILogger logger)
        {
            Manager = manager;
            Logger = logger;
        }


        [RateLimit("GetByType")]
        [ResponseCache(Location = ResponseCacheLocation.None)]
        [Route("products/{type}"), Route("{language}/{state}/products/{type}")]
        public async Task<HttpResponseMessage> GetByType(string type, string language = null, string state = null)
        {
            Logger.Log(() => "GetByType", Core.EventLevel.Trace);
            type = type?.TrimStart('@');
            var options = ProductOptionsParser.Parse(Request.GetQueryNameValuePairs());
            var stream = await Manager.GetProductsInTypeStream(type, options, language, state);
            return GetResponse(stream);
        }     

        [RateLimit("GetById"), Route("{language}/{state}/products/{id:int}"), Route("products/{id:int}")]
        [ResponseCache(Location = ResponseCacheLocation.Any, VaryByHeader = "fields", Duration = 600)]
        public async Task<HttpResponseMessage> GetById(string id, string language = null, string state = null)
        {
            var options = ProductOptionsParser.Parse(Request.GetQueryNameValuePairs());

            try
            {
                var elasticResponse = await Manager.FindStreamByIdAsync(id, options, language, state);
                return GetResponse(elasticResponse.Body, false);
            }
            catch (ElasticsearchClientException ex) when (ex.Response.HttpStatusCode == 404)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound) { Content = new StringContent("Product with id=" + id + " not found") };
            }
        }

        [Route("{language}/{state}/products/search"), Route("products/search"), RateLimit("Search"), HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None)]
        public async Task<HttpResponseMessage> Search([FromUri] string q, string language = null, string state = null)
        {
            var options = ProductOptionsParser.Parse(Request.GetQueryNameValuePairs());
            var stream = await Manager.SearchStreamAsync(q, options, language, state);
            return GetResponse(stream);
        }

        public HttpResponseMessage GetResponse(Stream stream, bool filter = true)
        {
            var response = Request.CreateResponse();
            response.StatusCode = HttpStatusCode.OK;

            if (filter)
            {
                response.Content = new PushStreamContent(
                    async (a, h, t) =>
                    await JsonFragmentExtractor.ExtractJsonFragment("_source", stream, a, 4)
                    );
            }
            else
            {
                response.Content = new StreamContent(stream);
            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };

            return response;
        }
    }
}