using System.Web.Mvc;

namespace QA.ProductCatalog.SiteSyncWebHost.ActionResults
{
    class ForbiddenActionResult : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.StatusCode = 403;
        }
    }
}