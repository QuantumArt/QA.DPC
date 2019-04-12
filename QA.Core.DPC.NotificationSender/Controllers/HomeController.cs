using Microsoft.AspNetCore.Mvc;

namespace QA.Core.DPC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/swagger");
        }    
    }
}