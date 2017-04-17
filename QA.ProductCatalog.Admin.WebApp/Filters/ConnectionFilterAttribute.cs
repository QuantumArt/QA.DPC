using QA.Core;
using QA.Core.DPC.QP.Servives;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp.Filters
{
    public class ConnectionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var customerCode = filterContext.HttpContext.Request.QueryString["customerCode"];
            ObjectFactoryBase.Resolve<ICustomerCodeProvider>().CustomerCode = customerCode;
            base.OnActionExecuting(filterContext);
        }
    }
}