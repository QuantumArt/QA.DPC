using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Elastic;
using NLog;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Route("api/healthCheck"),
     Route("api/{customerCode}/healthCheck"), 
     Route("api/{version:decimal}/healthCheck"),
     Route("api/{customerCode}/{version:decimal}/healthCheck")]
    public class HealthCheckController : Controller
    {
        private readonly ElasticConfiguration _elasticConfiguration;
        private readonly IHttpClientFactory _factory;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public HealthCheckController(ElasticConfiguration elasticConfiguration, IHttpClientFactory factory)
        {
            _elasticConfiguration = elasticConfiguration;
            _factory = factory;
        }

        public async Task<ActionResult> HealthCheck(bool isSync)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var httpClient = _factory.CreateClient();
            sb.AppendLine(@"Read\Write: " + Status(_elasticConfiguration.DataOptions.CanUpdate == isSync));
            foreach (var option in _elasticConfiguration.GetElasticIndices())
            {
                var uris = option.Url.Split(';').Select(n => n.Trim()).ToArray();
                var token = option.Token;
                foreach (var baseUri in uris)
                {
                    var uri = $"{baseUri}/{option.Name}";
                    bool isOk = false;
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Head, new Uri(uri));
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            request.Headers.Add("Authorization", $"Basic {token}");
                        }
                        var response = await httpClient.SendAsync(request);
                        isOk = response.IsSuccessStatusCode;
                        if (!isOk)
                        {
                            Logger.Error("Received HTTP status code '{code}' for url {url}", response.StatusCode, uri);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error while proceeding healthcheck with url {url}", uri);
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
