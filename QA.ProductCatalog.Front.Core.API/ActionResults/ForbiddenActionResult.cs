using Microsoft.AspNetCore.Mvc;

namespace QA.ProductCatalog.Front.Core.API.ActionResults
{
    class ForbiddenActionResult : ActionResult
    {
        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 403;
        }
    }
}