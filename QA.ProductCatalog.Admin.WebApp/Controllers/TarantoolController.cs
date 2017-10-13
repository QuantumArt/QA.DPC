using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    //[RequireCustomAction]
    public class TarantoolController : Controller
    {
        public Uri GetBaseUrl()
        {
            var key = ConfigurationManager.AppSettings["Tarantool.SyncApi"];
            if (!String.IsNullOrEmpty(key))
            {
                if (key.StartsWith("/"))
                {
                    key = $"{HttpContext.Request.Url?.Scheme}://{HttpContext.Request.Url?.Authority}{key}";
                }

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