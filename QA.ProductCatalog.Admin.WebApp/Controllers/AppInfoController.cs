using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class AppInfoController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
