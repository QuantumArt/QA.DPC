using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using QA.Core;
using QA.ProductCatalog.Admin.WebApp.App_Start;

namespace QA.ProductCatalog.Admin.WebApp
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            UnityConfig.Configure();
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            ModelBindersConfig.Register();

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            this.Error += MvcApplication_Error;
        }

        void MvcApplication_Error(object sender, EventArgs e)
        {
            var logger = ObjectFactoryBase.Resolve<ILogger>();
            var error = Server.GetLastError();
            if (error != null)
            {
                logger.ErrorException("An unhandled error", error);
            }
            else
            {
                logger.Error("An unhandled error without exception");
            }
        }
    }
}