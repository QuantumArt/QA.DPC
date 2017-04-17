using QA.Core;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Servives;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Filters
{
    public class IdentityFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var customerCode = filterContext.HttpContext.Request.QueryString["customerCode"];
            ObjectFactoryBase.Resolve<IIdentityProvider>().Identity = new Identity(customerCode);
            base.OnActionExecuting(filterContext);
        }
    }
}