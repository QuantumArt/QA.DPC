using Newtonsoft.Json;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using System;
using System.Linq;
using System.Web.Mvc;
using QA.Core.Web;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    public class AppInfoController : Controller
    {
        public ContentResult Index()
        {
            return Content(System.IO.File.GetLastWriteTime(typeof (AppInfoController).Assembly.Location).ToString());
        }

    }
}
