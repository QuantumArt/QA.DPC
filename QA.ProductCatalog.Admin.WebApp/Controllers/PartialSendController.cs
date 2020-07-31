using System.Collections.Generic;
using QA.Core.ProductCatalog.Actions.Actions;
using A = QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using QA.Core.DPC.Resources;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    public class PartialSendController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProvider _userProvider;
        private readonly ICompositeViewEngine _viewEngine;

        public PartialSendController(ITaskService taskService, IUserProvider userProvider, ICompositeViewEngine viewEngine)
        {
            _taskService = taskService;
            _viewEngine = viewEngine;
            _userProvider = userProvider;
        }

        public ActionResult Index(string[] ignoredStatus, bool localize = false, bool beta = false)
        {
            ViewBag.Localize = localize;
            ViewBag.IgnoredStatus = ignoredStatus ?? Enumerable.Empty<string>().ToArray();
            return View();
        }

        [HttpGet]
        public ActionResult Active()
        {
            var task = _taskService.GetLastTask(null, State.Running, typeof(SendProductAction).Name);
            if (task != null)
            {
                return Json(new { taskId = task.ID });
            }

            return Json(new { taskId = (object) null });
        }

        [HttpGet]
        public ActionResult Task(int taskId)
        {
            var task = _taskService.GetTask(taskId, true);
            bool taskProcessingFinished = task.StateID != (byte)State.Running && task.StateID != (byte)State.New;
            return Json(new { taskProcessingFinished, taskModel = new TaskModel(task) });
        }

        [HttpPost]
        public ActionResult Send(string idsStr, bool proceedIgnoredStatus, string[] ignoredStatus, bool stageOnly, bool localize = false)
        {
            int[] ids = null;
            ignoredStatus = ignoredStatus ?? Enumerable.Empty<string>().ToArray();

            ids = idsStr
                .Split(new[] { ' ', ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .Distinct()
                .ToArray();

            ViewBag.IgnoredStatus = ignoredStatus;
            ViewBag.Localize = localize;

            int userId = _userProvider.GetUserId();
            string userName = _userProvider.GetUserName();

            var parameters = new Dictionary<string, string>();
            if (!proceedIgnoredStatus)
            {
                parameters.Add("IgnoredStatus", string.Join(",", ignoredStatus));
            }

            if (stageOnly)
            {
                parameters.Add("skipPublishing", true.ToString());
                parameters.Add("skipLive", true.ToString());
            }

            parameters.Add("Localize", localize.ToString());

            string taskData = ActionData.Serialize(new ActionData
            {
                ActionContext = new A.ActionContext() { ContentItemIds = ids, ContentId = 288, Parameters = parameters, UserId = userId, UserName = userName }
            });

            var taskKey = typeof(SendProductAction).Name;
            int taskId = _taskService.AddTask(taskKey, taskData, userId, userName, TaskStrings.PartialSend);

            return Json(new { taskId });
        }
    }
}
