using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class AppInfoController : Controller
    {
        public ContentResult Index()
        {
            return Content(System.IO.File.GetLastWriteTime(typeof (AppInfoController).Assembly.Location).ToString());
        }

    }
}
