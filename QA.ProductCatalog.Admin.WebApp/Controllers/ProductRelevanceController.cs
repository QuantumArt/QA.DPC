using Microsoft.AspNetCore.Mvc;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    public class ProductRelevanceController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProvider _userProvider;

        public ProductRelevanceController(ITaskService taskService, IUserProvider userProvider)
        {
            _taskService = taskService;

            _userProvider = userProvider;
        }

        public ActionResult Index()
        {
            return View();
        }

       
    }
}