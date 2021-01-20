using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.Resources;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Consolidation(CustomerState state)
        {
            var template = MessageStrings.ConsolidationErrorMessage;
            var key = $"CustomerState{state}";
            var value = MessageStrings.ResourceManager.GetString(key);
            var message = string.Format(template, value);
            return View("Consolidation", message);
        }
    }
}
