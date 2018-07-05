using System.Web.Mvc;

namespace QA.ProductCatalog.SiteSyncWebHost
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}