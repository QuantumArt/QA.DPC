using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Route("sync"), Route("api/sync"), Route("api/{version:decimal}/sync"), 
        Route("api/{customerCode}/{version:decimal}/sync"), Route("api/{customerCode}/sync")]
    public class SyncController : BaseController
    {
        private const int LockTimeoutInMs = 1000;

        private readonly ITaskService _taskService;
        
        public SyncController(
            ILoggerFactory loggerFactory, 
            ProductManager manager, 
            ElasticConfiguration configuration, 
            ITaskService taskService 
        ) : base(manager, loggerFactory, configuration)
        {
            _taskService = taskService;
        }

        [HttpPut]
        [Route("{language}/{state}")]
        public async Task<IActionResult> Put([FromBody]PushMessage message, string language, string state, string instanceId = null)
        {
            if (!ValidateInstance(instanceId, Configuration.DataOptions.InstanceId))
            {
                return CreateUnauthorizedResult(instanceId, Configuration.DataOptions.InstanceId);
            }

            var syncer = Configuration.GetSyncer(language, state);
            var product = message.Product;
            string id = Manager.GetProductId(message.Product);

            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest($"Невозможно создать или обновить продукт {id}. Данный экземпляр API предназначен только для чтения.");
            }


            Logger.LogInformation($"Получен запрос на обновление/добавление продукта: {id}");

            if (await syncer.EnterSingleCrudAsync(LockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.CreateAsync(product, message.RegionTags, language, state);

                    return CreateResult(result, Logger);
                }
                finally
                {
                    syncer.LeaveSingleCrud();
                }
            }
            else
                return BadRequest($"Не удалось войти в EnterSingleCRUDAsync в течение {LockTimeoutInMs} миллисекунд");
        }

        [HttpDelete]
        [Route("{language}/{state}")]
        public async Task<object> Delete([FromBody]PushMessage message, string language, string state, string instanceId = null)
        {
            if (!ValidateInstance(instanceId, Configuration.DataOptions.InstanceId))
            {
                return CreateUnauthorizedResult(instanceId, Configuration.DataOptions.InstanceId);
            }

            var syncer = Configuration.GetSyncer(language, state);
            var product = message.Product;

            var id = Manager.GetProductId(message.Product);

            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest($"Невозможно удалить продукт {id}. Данный экземпляр API предназначен только для чтения.");
            }

            Logger.LogInformation("Получен запрос на удаление продукта: " + id);

            if (await syncer.EnterSingleCrudAsync(LockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.DeleteAsync(product, language, state);
                    return CreateResult(result, Logger);
                }
                finally
                {
                    syncer.LeaveSingleCrud();
                }
            }
            else
                return BadRequest($"Не удалось войти в EnterSingleCRUDAsync в течение {LockTimeoutInMs} миллисекунд");
        }

        [Route("{language}/{state}/reset"), HttpPost]
        public ActionResult Reset(string language, string state)
        {

            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest("Невозможно выполнить операцию пересоздания индекса. Данный экземпляр API предназначен только для чтения.");
            }

            var syncer = Configuration.GetSyncer(language, state);

            if (!syncer.AnySlotsLeft)
                return BadRequest("Нет свободных слотов, дождитесь завершения предыдущих операций");

            int taskId = _taskService.AddTask("ReindexAllTask", $"{language}/{state}", 0, null, "ReindexAllTask");
            
            return Json(new JObject(new JProperty("taskId", taskId)));
        }

        [Route("task"), HttpGet]
        public QA.Core.ProductCatalog.ActionsRunnerModel.Task Task(int id)
        {
            return _taskService.GetTask(id);
        }

        [Route("settings"), HttpGet]
        public TaskItem[] Settings()
        {
            int count;
            var tasks = _taskService.GetTasks(0, int.MaxValue, null, null, null, null, out count);
            var lastTasks = tasks.GroupBy(t => t.Data).Select(g => g.OrderByDescending(t => t.ID).First()).ToArray();

            var r =
                from o in Configuration.GetElasticIndices()
                join t in lastTasks on $"{o.Language}/{o.State}" equals t.Data into ts
                from t in ts.DefaultIfEmpty()
                select new TaskItem
                {
                    ChannelLanguage = o.Language,
                    ChannelState = o.State,
                    ChannelDate = o.Date,
                    IsDefault = o.IsDefault,                    
                    TaskId = t?.ID,
                    TaskProgress = t?.Progress,
                    TaskState = (State?)t?.StateID,
                    TaskStart = t?.CreatedTime,
                    TaskEnd = t?.LastStatusChangeTime,
                    TaskMessage = t?.Message
                };

            return r.ToArray();
        }

        private static IActionResult CreateResult(SonicResult results, ILogger logger)
        {
            if (!results.Succeeded)
            {
                logger.LogError(results.ToString(), results.GetException());
                return new ContentResult() { Content = results.ToString(), StatusCode = 500 };
            }

            return new OkResult();
        }

        private IActionResult CreateUnauthorizedResult(string instanceId, string actualInstanceId)
        {
            Logger.LogInformation($"InstanceId {instanceId} указан неверно, должен быть {actualInstanceId}");            
            return new UnauthorizedResult();
        }

        private bool ValidateInstance(string instanceId, string actualInstanceId)
        {
            return instanceId == actualInstanceId || string.IsNullOrEmpty(instanceId) && string.IsNullOrEmpty(actualInstanceId);
        }   
    }
}
