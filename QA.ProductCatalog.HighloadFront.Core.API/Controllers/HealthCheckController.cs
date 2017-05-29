using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Route("api/HealthCheck")]
    public class HealthCheckController : Controller
    {
        private readonly DataOptions _dataOptions;

        public HealthCheckController(IOptions<DataOptions> optionsAccessor)
        {
            _dataOptions = optionsAccessor.Value;
        }

        public async Task<HttpResponseMessage> HealthCheck(bool isSync)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var httpClient = new HttpClient();
            sb.AppendLine(@"Read\Write: " + Status(_dataOptions.CanUpdate == isSync));

            foreach (var option in _dataOptions.Elastic)
            {

                var uri = $"{option.Adress}/{option.Index}";
                var ok = false;
                try
                {

                    var request = new HttpRequestMessage(HttpMethod.Head, new Uri(uri));
                    var response = await httpClient.SendAsync(request);
                    ok = response.StatusCode == HttpStatusCode.OK;

                }
                catch (HttpRequestException)
                {
                }

                sb.AppendLine($@"Index '{uri}': " + Status(ok));
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(sb.ToString())
            };
        }

        private static string Status(bool flag)
        {
            return flag ? "OK" : "Error";
        }

        [HttpGet]
        [Route("sync")]
        public async Task<HttpResponseMessage> Sync()
        {
            return await HealthCheck(true);
        }


        [HttpGet]
        [Route("search")]
        public async Task<HttpResponseMessage> Search()
        {
            return await HealthCheck(false);
        }
    }
}
