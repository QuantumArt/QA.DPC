﻿using System.Web;
using System.Web.Mvc;

namespace QA.ProductCatalog.Admin.WebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleAndLogErrorAttribute { View = "Error" });
        }
    }
}