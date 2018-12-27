using System.Web.Script.Serialization;
using Newtonsoft.Json;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using System;
using System.Linq;
using System.Web.Mvc;
using QA.Core.Web;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [RequireCustomAction]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProvider _userProvider;

        public TaskController(ITaskService taskService, IUserProvider userProvider)
        {
            _taskService = taskService;

            _userProvider = userProvider;
        }

        public ViewResult Index(bool? showOnlyMine, bool? notify, bool allowSchedule = false)
        {
            var tasksPageInfo = new TasksPageInfo { ShowOnlyMine = showOnlyMine == true, Notify = notify == true, States = _taskService.GetAllStates(), AllowSchedule = allowSchedule };

            return View(tasksPageInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="showOnlyMine"></param>
        /// <param name="filterJson">типа такого [{"field":"StateId","operator":"eq","value":"3"},{"field":"DisplayName","operator":"contains","value":"asdfsdf"}]</param>
        /// <returns></returns>
        public string TasksData(int skip, int take, bool showOnlyMine, string filterJson)
        {
            int? stateIdToFilterBy = null;

            string nameFillter = null;

	        bool? hasSchedule = null;

            if (filterJson != null)
            {
                var filters = new JavaScriptSerializer().Deserialize<KendoGridFilter[]>(filterJson);

                foreach (var filter in filters)
                {
                    if (filter.field == "StateId")
                        stateIdToFilterBy = (int)filter.value;
                    else if (filter.field == "DisplayName")
                        nameFillter = filter.value.ToString();
					else if (filter.field == "HasSchedule")
						hasSchedule = (bool) filter.value;
                }
            }

            int userId = _userProvider.GetUserId();

            int? userIdToFilterBy = showOnlyMine ? userId : (int?)null;

            int totalCount;

            var tasks = _taskService.GetTasks(skip, take, userIdToFilterBy, stateIdToFilterBy, nameFillter, hasSchedule,out totalCount);

            TaskModel myLastTask = null;

            if (showOnlyMine)
            {
                var lastTask = skip == 0 && nameFillter == null && !stateIdToFilterBy.HasValue && tasks.Length > 0 ? tasks.First() : _taskService.GetLastTask(userId);

                myLastTask = Type.GetType(lastTask.Name) == null
                    ? new CustomActionTaskModel(lastTask)
                    : new TaskModel(lastTask);
            }

            string dataJsonStr = JsonConvert.SerializeObject(
                new
                {
                    tasks = tasks.Select(x => new TaskModel(x)),
                    totalTasks = totalCount,
                    myLastTaskHtml = myLastTask == null ? null : this.RenderRazorViewToString("ActionProps", myLastTask)
                });

            return string.Format("{{\"hashCode\":{1},\"data\":{0}}}", dataJsonStr, dataJsonStr.GetHashCode());
        }

        public ActionResult MyLastTaskInfo()
        {
            var task = _taskService.GetLastTask(_userProvider.GetUserId());

            if (task == null)
                return null;
            else
                return
                    PartialView("ActionProps", Type.GetType(task.Name) == null
                        ? new CustomActionTaskModel(task)
                        : new TaskModel(task));
        }

        public ActionResult Cancel(int taskId)
        {
            bool isCancelled = _taskService.Cancel(taskId);

            return Json(isCancelled, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Rerun(int taskId)
        {
            bool success = _taskService.Rerun(taskId);

            return Json(success, JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult Schedule(int taskId)
        {
            var task = _taskService.GetTask(taskId);

            ViewBag.OnSuccess = "scheduleSaved";

            return
                PartialView(task.Schedule == null
                    ? new TaskScheduleModel { TaskId = taskId }
                    : new TaskScheduleModel(task.Schedule));
        }

        public string SaveShedule(TaskScheduleModel schedule)
        {
            _taskService.SaveSchedule(schedule.TaskId, schedule.Enabled, schedule.CronExpression);

            return "Сохранено";
        }
    }
}
