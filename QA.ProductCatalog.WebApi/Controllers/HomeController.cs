using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.WebApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/swagger");
        }    
    }
}