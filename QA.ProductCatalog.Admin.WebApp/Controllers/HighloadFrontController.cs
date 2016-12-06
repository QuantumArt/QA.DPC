using QA.Core.Web;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class HighloadFrontController : Controller
	{
        private const string BaseUrl = "http://mscservices01:8082/";

        public HighloadFrontController()
		{
		}

        [HttpGet]
		public ActionResult Index(string url)
		{
            return View();
		}

        [HttpGet]
        public async Task<ActionResult> GetSettings(string url)
        {
            using (var client = new HttpClient())
            {
                return GetJson(await client.GetStringAsync(BaseUrl + url));
            }
        }

        [HttpPost]
        public async Task<ActionResult> IndexChanel(string url)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(BaseUrl + url, null);
                return GetJson(await response.Content.ReadAsStringAsync());
            }            
        }

        private ContentResult GetJson(string json)
        {
            return Content(json, "application/json");
        }
    }
}