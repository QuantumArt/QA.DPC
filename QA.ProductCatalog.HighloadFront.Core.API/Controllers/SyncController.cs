using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.Logger;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Options;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Route("sync"), Route("api/sync"), Route("api/{version:decimal}/sync"), 
        Route("api/{customerCode}/{version:decimal}/sync"), Route("api/{customerCode}/sync")]
    public class SyncController : Controller
    {
        private ILogger Logger { get; }
        private ProductManager Manager { get; }


        private const int LockTimeoutInMs = 1000;

        private readonly IElasticConfiguration _configuration;
        private readonly ITaskService _taskService;
        private readonly DataOptions _dataOptions;

        public SyncController(ILogger logger, ProductManager manager, IElasticConfiguration configuration, ITaskService taskService, IOptions<DataOptions> optionsAccessor)
        {
            Logger = logger;
            Manager = manager;
            _configuration = configuration;
            _taskService = taskService;
            _dataOptions = optionsAccessor.Value;
        }

        [HttpPut]
        [Route("{language}/{state}")]
        public async Task<IActionResult> Put([FromBody]PushMessage message, string language, string state, string instanceId = null)
        {
            if (!ValidateInstance(instanceId, _dataOptions.InstanceId))
            {
                return CreateUnauthorizedResult(instanceId, _dataOptions.InstanceId);
            }

            var syncer = _configuration.GetSyncer(language, state);
            var product = message.Product;
            string id = Manager.GetProductId(message.Product);

            if (!_dataOptions.CanUpdate)
            {
                throw new Exception($"Невозможно создать или обновить продукт {id}. Данный экземпляр API предназначен только для чтения.");
            }


            Logger.Info($"Получен запрос на обновление/добавление продукта: {id}");

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
                throw new Exception($"Не удалось войти в EnterSingleCRUDAsync в течение {LockTimeoutInMs} миллисекунд");
        }

        [HttpDelete]
        [Route("{language}/{state}")]
        public async Task<object> Delete([FromBody]PushMessage message, string language, string state, string instanceId = null)
        {
            if (!ValidateInstance(instanceId, _dataOptions.InstanceId))
            {
                return CreateUnauthorizedResult(instanceId, _dataOptions.InstanceId);
            }

            var syncer = _configuration.GetSyncer(language, state);
            var product = message.Product;

            var id = Manager.GetProductId(message.Product);

            if (!_dataOptions.CanUpdate)
            {
                throw new Exception($"Невозможно удалить продукт {id}. Данный экземпляр API предназначен только для чтения.");
            }

            Logger.Info("Получен запрос на удаление продукта: " + id);

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
                throw new Exception($"Не удалось войти в EnterSingleCRUDAsync в течение {LockTimeoutInMs} миллисекунд");
        }

        [Route("{language}/{state}/reset"), HttpPost]
        public HttpResponseMessage Reset(string language, string state)
        {

            if (!_dataOptions.CanUpdate)
            {
                throw new Exception("Невозможно выполнить операцию пересоздания индекса. Данный экземпляр API предназначен только для чтения.");
            }

            var syncer = _configuration.GetSyncer(language, state);

            if (!syncer.AnySlotsLeft)
                throw new Exception("Нет свободных слотов, дождитесь завершения предыдущих операций");

            int taskId = _taskService.AddTask("ReindexAllTask", $"{language}/{state}", 0, null, "ReindexAllTask");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ \"taskId\":" + taskId + " }")
            };
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
                from o in _configuration.GetElasticIndices()
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
                logger.ErrorException(results.ToString(), results.GetException());
                return new ContentResult() { Content = results.ToString(), StatusCode = 500 };
            }

            return new OkResult();
        }

        private IActionResult CreateUnauthorizedResult(string instanceId, string actualInstanceId)
        {
            Logger.LogInfo(() => $"InstanceId {instanceId} указан неверно, должен быть {actualInstanceId}");            
            return new UnauthorizedResult();
        }

        private bool ValidateInstance(string instanceId, string actualInstanceId)
        {
            return instanceId == actualInstanceId || string.IsNullOrEmpty(instanceId) && string.IsNullOrEmpty(actualInstanceId);
        }   
    }
}
