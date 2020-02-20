using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace QA.ProductCatalog.Front.Core.API.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return Redirect("/swagger");
        }    
    }
}