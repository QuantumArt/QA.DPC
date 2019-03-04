using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.ProductCatalog.Admin.WebApp.Filters;
using QA.ProductCatalog.Integration;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    public class TarantoolController : Controller
    {
        
        private HttpContext _httpContext;
        
        private IntegrationProperties _options;
        
        public TarantoolController(IHttpContextAccessor httpContextAccessor, IOptions<IntegrationProperties> options)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _options = options.Value;
        }
        
        public Uri GetBaseUrl()
        {
            var key = _options.TarantoolSyncUrl;
            
            if (string.IsNullOrEmpty(key)) return null;
            
            if (key.StartsWith("/"))
            {
                key = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}{key}";
            }
            return new Uri(key);
        }

        [HttpGet]
        public ActionResult Index(string customerCode)
        {
            ViewBag.CustomerCode = customerCode;
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> Get(string customerCode)
        {
            var uri = new Uri(GetBaseUrl(), $"api/Import/{customerCode}/get");

            using (var client = new HttpClient())
            {
                return GetJson(await client.GetStringAsync(uri));
            }
        }

        [HttpPost]
        public async Task<ActionResult> Import(string customerCode)
        {
            var uri = new Uri(GetBaseUrl(), $"api/Import/{customerCode}/start");
            
            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(uri, null);
                return GetJson(await result.Content.ReadAsStringAsync());
            }
        }

        [HttpPost]
        public async Task<ActionResult> Stop(string customerCode)
        {
            var uri = new Uri(GetBaseUrl(), $"/dpc.tarantool/api/Import/{customerCode}/stop");

            using (var client = new HttpClient())
            {
                var result = await client.PostAsync(uri, null);
                return GetJson(await result.Content.ReadAsStringAsync());
            }
        }

        private ContentResult GetJson(string json)
        {
            return Content(json, "application/json");
        }
    }
}