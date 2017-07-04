using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class HighloadFrontController : Controller
	{

        public Uri GetBaseUrl()
        {
            var key = ConfigurationManager.AppSettings["HighloadFront.SyncApi"];
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
        //[RequireCustomAction]
        public ActionResult Index(string customerCode)
        {
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