using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using QA.Core;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.App_Core;
using QA.ProductCatalog.HighloadFront.Models;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Infrastructure;

namespace QA.ProductCatalog.HighloadFront.Controllers
{
    [RoutePrefix("sync")]
    public class SyncController : ApiController
    {

        private ILogger Logger { get; }
        private ProductManager Manager { get; }


        private const int _lockTimeoutInMs = 1000;

        private readonly Func<string, string, IndexOperationSyncer> _getSyncer;
        private readonly ITaskService _taskService;
        private readonly DataOptions _dataOptions;

        public SyncController(ILogger logger, ProductManager manager, Func<string, string, IndexOperationSyncer> getSyncer, ITaskService taskService, IOptions<DataOptions> optionsAccessor)
        {
            Logger = logger;
            Manager = manager;
            _getSyncer = getSyncer;
            _taskService = taskService;
            _dataOptions = optionsAccessor.Value;
        }

        [Route("{language}/{state}")]
        public async Task<HttpResponseMessage> Put([FromBody]PushMessage message, string language, string state)
        {
            var syncer = _getSyncer(language, state);
            var product = message.Product;

            string id = Manager.GetProductId(message.Product);

            Logger.Info($"Получен запрос на обновление/добавление продукта: {id}");

            if (await syncer.EnterSingleCRUDAsync(_lockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.CreateAsync(product, message.RegionTags, language, state);

                    return CreateResult(result, Logger);
                }
                finally
                {
                    syncer.LeaveSingleCRUD();
                }
            }
            else
                throw new Exception($"Не удалось войти в EnterSingleCRUDAsync в течение {_lockTimeoutInMs} миллисекунд");
        }

        [Route("{language}/{state}")]
        public async Task<object> Delete([FromBody]PushMessage message, string language, string state)
        {
            var syncer = _getSyncer(language, state);
            var product = message.Product;

            var id = Manager.GetProductId(message.Product);

            Logger.Info("Получен запрос на удаление продукта: " + id);

            if (await syncer.EnterSingleCRUDAsync(_lockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.DeleteAsync(product, language, state);

                    return CreateResult(result, Logger);
                }
                finally
                {
                    syncer.LeaveSingleCRUD();
                }
            }
            else
                throw new Exception($"Не удалось войти в EnterSingleCRUDAsync в течение {_lockTimeoutInMs} миллисекунд");
        }

        [Route("{language}/{state}/reset"), HttpPost]
        public HttpResponseMessage Reset(string language, string state)
        {
            var syncer = _getSyncer(language, state);

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
                from o in _dataOptions.Elastic
                join t in lastTasks on $"{o.Language}/{o.State}" equals t.Data into ts
                from t in ts.DefaultIfEmpty()
                select new TaskItem
                {
                    ChannelLanguage = o.Language,
                    ChannelState = o.State,
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

        private static HttpResponseMessage CreateResult(SonicResult results, ILogger logger)
        {
            if (!results.Succeeded)
            {
                logger.ErrorException(results.ToString(), results.GetException());

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(results.ToString())
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
