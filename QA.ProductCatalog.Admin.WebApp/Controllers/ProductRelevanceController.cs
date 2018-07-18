using System.Web.Mvc;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.Web;
using QA.ProductCatalog.Infrastructure;

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

        public ViewResult Index()
        {
            return View();
        }

       
    }
}