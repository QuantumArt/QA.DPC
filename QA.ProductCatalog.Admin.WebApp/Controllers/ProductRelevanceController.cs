using System.Linq;
using System.Web.Mvc;
using QA.Core.DPC.Loader;
using QA.Core.ProductCatalog.Actions.Actions;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.Web;
using QA.ProductCatalog.Infrastructure;
using Helpers = QA.Core.ProductCatalog.ActionsRunnerModel.Helpers;

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