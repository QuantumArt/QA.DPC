using QA.Core.Web;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class HighloadFrontController : Controller
	{
        private readonly Uri BaseUri;

        public HighloadFrontController()
		{
            BaseUri = new Uri(ConfigurationManager.AppSettings["HighloadFront.SyncApi"]);
        }

        [HttpGet]
        //[RequireCustomAction]
        public ActionResult Index(string url)
		{
            return View();
		}

        [HttpGet]
        public async Task<ActionResult> GetSettings(string url)
        {
            var uri = new Uri(BaseUri, url);

            using (var client = new HttpClient())
            {
                return GetJson(await client.GetStringAsync(uri));
            }
        }

        [HttpPost]
        public async Task<ActionResult> IndexChanel(string url)
        {
            var uri = new Uri(BaseUri, url);

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(uri, null);
                return GetJson(await response.Content.ReadAsStringAsync());
            }            
        }

        private ContentResult GetJson(string json)
        {
            return Content(json, "application/json");
        }
    }
}