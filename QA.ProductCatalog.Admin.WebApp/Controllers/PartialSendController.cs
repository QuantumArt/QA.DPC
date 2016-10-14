using System.Collections.Generic;
using System.Threading;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions;
using QA.Core.ProductCatalog.Actions.Tasks;
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
    [RequireCustomAction]
    public class PartialSendController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProvider _userProvider;

        public PartialSendController(ITaskService taskService, IUserProvider userProvider)
        {
            _taskService = taskService;

            _userProvider = userProvider;
        }

        public ViewResult Index(string[] IgnoredStatus, bool localize = false)
        {
            ViewBag.Localize = localize;
            ViewBag.IgnoredStatus = IgnoredStatus ?? Enumerable.Empty<string>().ToArray();  
			return View();
        }

        public PartialViewResult CurrentActiveTask()
        {
			var task = _taskService.GetLastTask(null, State.Running, typeof(SendProductAction).Name);

            if (task != null)
                return PartialView("ActionProps", new TaskModel(task));
            else
                return null;
        }

        public ActionResult UserTask(int taskId)
        {
            var task = _taskService.GetTask(taskId);

            bool taskProcessingFinished = task.StateID != (byte)State.Running && task.StateID != (byte)State.New;

            return Json(new { taskProcessingFinished, taskHtml = this.RenderRazorViewToString("ActionProps", new TaskModel(task)) }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Send(string idsStr, bool proceedIgnoredStatus, string[] IgnoredStatus, bool stageOnly, bool localize = false)
        {
            int[] ids = null;
			IgnoredStatus = IgnoredStatus ??  Enumerable.Empty<string>().ToArray();

            try
            {
                ids = idsStr
                        .Split(new[] { ' ', ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .Distinct()
                        .ToArray();

				if (ids.Length == 0)
					ModelState.AddModelError("idsStr", "Список ID не может быть пустым");

            }
            catch
            {
                ModelState.AddModelError("idsStr", "Введен некорректный список ID");
            }

			ViewBag.IgnoredStatus = IgnoredStatus;
            ViewBag.Localize = localize;


            if (ModelState.IsValid)
            {
                int userId = _userProvider.GetUserId();
				string userName = _userProvider.GetUserName();

				var parameters = new Dictionary<string, string>();
				if (!proceedIgnoredStatus)
				{
					parameters.Add("IgnoredStatus", string.Join(",", IgnoredStatus));
				}

                if (stageOnly)
                {
                    parameters.Add("skipPublishing", true.ToString());
                    parameters.Add("skipLive", true.ToString()); 
                }

                parameters.Add("Localize", localize.ToString());

                string taskData =
		            ActionData.Serialize(new ActionData
		            {
			            ActionContext =
				            new ActionContext {ContentItemIds = ids, ContentId = 288, Parameters = parameters, UserId = userId, UserName = userName}
		            });

				var taskKey = typeof(SendProductAction).Name;

                int taskId = _taskService.AddTask(taskKey, taskData, userId, userName, "Частичная отправка");

				return View("Result", taskId);
            }
            else
            {
				return View("Index");
            }
        }

       
    }
}
