using Newtonsoft.Json;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using QA.ProductCatalog.Admin.WebApp.Models;
using QA.ProductCatalog.ContentProviders;
using QA.ProductCatalog.Infrastructure;
using QA.ProductCatalog.Admin.WebApp.Core;
using QA.ProductCatalog.Admin.WebApp.Filters;

namespace QA.ProductCatalog.Admin.WebApp.Controllers
{
    [Route("Task")]
    [RequireCustomAction]
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IUserProvider _userProvider;
        private readonly ICompositeViewEngine _viewEngine;

        public TaskController(ITaskService taskService, IUserProvider userProvider, ICompositeViewEngine viewEngine)
        {
            _taskService = taskService;
            _viewEngine = viewEngine;
            _userProvider = userProvider;
        }

        [HttpGet]
        public ActionResult Index(bool? showOnlyMine, bool? notify, bool allowSchedule = false)
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
        [HttpGet("TasksData")]
        public async Task<ActionResult> TasksData(int skip, int take, bool showOnlyMine, string filterJson)
        {
            int? stateIdToFilterBy = null;

            string nameFillter = null;

	        bool? hasSchedule = null;

            if (filterJson != null)
            {
                var filters = JsonConvert.DeserializeObject<KendoGridFilter[]>(filterJson);

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
                    myLastTaskHtml = myLastTask == null ? null : await this.RenderRazorViewToString(_viewEngine, "ActionProps", myLastTask)
                });

            return new ContentResult() { 
                ContentType = "application/json", 
                Content= $"{{\"hashCode\":{dataJsonStr.GetHashCode()},\"data\":{dataJsonStr}}}"
            };
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

        [HttpPost("Cancel")]
        public ActionResult Cancel(int taskId)
        {
            bool isCancelled = _taskService.Cancel(taskId);

            return Json(isCancelled);
        }

        [HttpPost("Rerun")]
        public ActionResult Rerun(int taskId)
        {
            bool success = _taskService.Rerun(taskId);

            return Json(success);
        }

        [HttpGet("Schedule")]
        public ActionResult Schedule(int taskId)
        {
            var task = _taskService.GetTask(taskId);

            ViewBag.OnSuccess = "scheduleSaved";

            return
                PartialView(task.Schedule == null
                    ? new TaskScheduleModel { TaskId = taskId }
                    : new TaskScheduleModel(task.Schedule));
        }

        [HttpPost("SaveSchedule")]
        public string SaveSchedule(TaskScheduleModel schedule)
        {
            _taskService.SaveSchedule(schedule.TaskId, schedule.Enabled, schedule.CronExpression);

            return "Сохранено";
        }
    }
}
