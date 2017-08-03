using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QA.ProductCatalog.WebApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: HelpPage/Home
        public ActionResult Index()
        {
            return RedirectToRoute(new { area = "HelpPage", controller = "Help", action = "Index" });
        }
    }
}