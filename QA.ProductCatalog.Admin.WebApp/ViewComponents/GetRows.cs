using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.UI;
using QA.ProductCatalog.Admin.WebApp.Models;

namespace QA.ProductCatalog.Admin.WebApp.ViewComponents
{
    public class GetRows : ViewComponent
    {
        public IViewComponentResult Invoke(GetRowsModel model)
        {
            return View(model);
        }
    }
}