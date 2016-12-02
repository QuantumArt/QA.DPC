using QA.Core.Web;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
	public class HighloadFrontController : Controller
	{
		public HighloadFrontController()
		{
		}
		public ActionResult Index(string url)
		{
            ViewBag.Url = url;
            return View();
		}	
	}
}