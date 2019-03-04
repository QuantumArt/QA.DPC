using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.DPC.Core.Helpers;
using QA.ProductCatalog.Admin.WebApp.Filters;
using QA.ProductCatalog.Integration;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class HighloadFrontController : Controller
    {

        private HttpContext _httpContext;
        
        private IntegrationProperties _options;

        private readonly QPHelper _qpHelper;

        public HighloadFrontController(IHttpContextAccessor httpContextAccessor, IOptions<IntegrationProperties> options, QPHelper helper)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _options = options.Value;
            _qpHelper = helper;
        }

        public Uri GetBaseUrl()
        {
            
            var key = _options.HighloadFrontSyncUrl;
            
            if (string.IsNullOrEmpty(key)) return null;
            
            if (key.StartsWith("/"))
            {
                key = $"{_httpContext.Request.Scheme}://{_httpContext.Request.Host}{key}";
            }
            return new Uri(key);
        }

        [HttpGet]
        [RequireCustomAction]
        public ActionResult Index(string customerCode)
        {
            ViewBag.HostId = _qpHelper.HostId;            
            ViewBag.CustomerCode = customerCode;
            return View();
		}

        [HttpGet]
        public async Task<ActionResult> GetSettings(string url, string customerCode)
        {
            var uri = new Uri(GetBaseUrl(), url);
            var s = $"{uri}?customerCode={customerCode}";
            using (var client = new HttpClient())
            {
                return GetJson(await client.GetStringAsync(s));
            }
        }

        [HttpPost]
        public async Task<ActionResult> IndexChanel(string url, string customerCode)
        {
            var uri = new Uri(GetBaseUrl(), url);
            var s = $"{uri}?customerCode={customerCode}";
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(s, null);
                return GetJson(await response.Content.ReadAsStringAsync());
            }            
        }

        private ContentResult GetJson(string json)
        {
            return Content(json, "application/json");
        }
    }
}