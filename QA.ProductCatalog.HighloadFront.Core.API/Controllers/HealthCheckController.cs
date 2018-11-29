using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Elastic;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.HighloadFront.Options;


namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Route("api/{customerCode}/healthCheck"), Route("api/healthCheck")]
    public class HealthCheckController : Controller
    {
        private readonly IElasticConfiguration _elasticConfiguration;

        private readonly DataOptions _options;

        public HealthCheckController(IElasticConfiguration elasticConfiguration, DataOptions options)
        {
            _elasticConfiguration = elasticConfiguration;
            _options = options;
        }

        public async Task<ActionResult> HealthCheck(bool isSync)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var httpClient = new HttpClient();
            sb.AppendLine(@"Read\Write: " + Status(_options.CanUpdate == isSync));
            foreach (var option in _elasticConfiguration.GetElasticIndices())
            {
                var uris = option.Url.Split(';').Select(n => n.Trim()).ToArray();
                foreach (var baseUri in uris)
                {
                    var uri = $"{baseUri}/{option.Name}";
                    bool isOk;
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Head, new Uri(uri));
                        var response = await httpClient.SendAsync(request);
                        isOk = response.StatusCode == HttpStatusCode.OK;

                    }
                    catch (HttpRequestException)
                    {
                        isOk = false;
                    }

                    sb.AppendLine($@"Index '{uri}': " + Status(isOk));
                }
            }

            return Content(sb.ToString(), "text/plain");
        }

        private static string Status(bool flag)
        {
            return flag ? "OK" : "Error";
        }

        [HttpGet]
        [Route("sync")]
        public async Task<ActionResult> Sync()
        {
            return await HealthCheck(true);
        }


        [HttpGet]
        [Route("search")]
        public async Task<ActionResult> Search()
        {
            return await HealthCheck(false);
        }
    }
}
