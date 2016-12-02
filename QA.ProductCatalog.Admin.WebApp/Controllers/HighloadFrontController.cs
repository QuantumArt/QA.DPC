using QA.Core.Web;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
	public class HighloadFrontController : Controller
	{
		public HighloadFrontController()
		{
		}
		public ActionResult Index(string url)
		{
            return View("Index", url);
		}	
	}
}