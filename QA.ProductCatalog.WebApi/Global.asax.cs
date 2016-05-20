using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using QA.Core;

namespace QA.ProductCatalog.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

			Error += WebApiApplication_Error;
        }

		void WebApiApplication_Error(object sender, EventArgs e)
		{
			var logger = ObjectFactoryBase.Resolve<ILogger>();
			
			var error = Server.GetLastError();
			
			if (error != null)
				logger.ErrorException("An unhandled error", error);
			else
				logger.Error("An unhandled error without exception");
		}
    }
}
