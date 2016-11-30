using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using QA.Core;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.App_Core;
using QA.ProductCatalog.HighloadFront.Models;

namespace QA.ProductCatalog.HighloadFront.Controllers
{
    [RoutePrefix("sync")]
    public class SyncController : ApiController
    {

        private ILogger Logger { get; }
        private ProductManager Manager { get; }


        private const int _lockTimeoutInMs = 1000;

        private readonly IndexOperationSyncer _syncer;
        private readonly ITaskService _taskService;

        public SyncController(ILogger logger, ProductManager manager, IndexOperationSyncer syncer, ITaskService taskService)
        {
            Logger = logger;
            Manager = manager;
            _syncer = syncer;
            _taskService = taskService;
        }

        [Route("{language}/{state}")]
        public async Task<HttpResponseMessage> Put([FromBody]PushMessage message, string language = "invariant", string state = "live")
        {
            var product = message.Product;

            string id = Manager.GetProductId(message.Product);

            Logger.Info($"Получен запрос на обновление/добавление продукта: {id}");

            if (await _syncer.EnterSingleCRUDAsync(_lockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.CreateAsync(product, language, state);

                    return CreateResult(result, Logger);
                }
                finally
                {
                    _syncer.LeaveSingleCRUD();
                }
            }
            else
                throw new Exception($"Не удалось войти в EnterSingleCRUDAsync в течение {_lockTimeoutInMs} миллисекунд");
        }



        [Route("{language}/{state}")]
        public async Task<object> Delete([FromBody]PushMessage message, string language = "invariant", string state = "live")
        {
            var product = message.Product;

            var id = Manager.GetProductId(message.Product);

            Logger.Info("Получен запрос на удаление продукта: " + id);

            if (await _syncer.EnterSingleCRUDAsync(_lockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.DeleteAsync(product, language, state);

                    return CreateResult(result, Logger);
                }
                finally
                {
                    _syncer.LeaveSingleCRUD();
                }
            }
            else
                throw new Exception($"Не удалось войти в EnterSingleCRUDAsync в течение {_lockTimeoutInMs} миллисекунд");
        }

        [Route("{language}/{state}/reset"), HttpPost]
        public HttpResponseMessage Reset(string language = "invariant", string state = "live")
        {
            if (!_syncer.AnySlotsLeft)
                throw new Exception("Нет свободных слотов, дождитесь завершения предыдущих операций");

            int taskId = _taskService.AddTask("ReindexAllTask", $"{language}/{state}", 0, null, "ReindexAllTask");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"Background task id={taskId} was created...")
            };
        }

        [Route("task"), HttpGet]
        public QA.Core.ProductCatalog.ActionsRunnerModel.Task Task(int id)
        {
            return _taskService.GetTask(id);
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
