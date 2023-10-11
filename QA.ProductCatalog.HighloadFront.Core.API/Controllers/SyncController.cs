using System;
using Microsoft.AspNetCore.Mvc;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.ProductCatalog.HighloadFront.Elastic;
using QA.ProductCatalog.HighloadFront.Models;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using QA.Core.ProductCatalog.ActionsRunner;

namespace QA.ProductCatalog.HighloadFront.Core.API.Controllers
{
    [Route("sync"), Route("api/sync"), Route("api/{version:decimal}/sync"), 
        Route("api/{customerCode}/{version:decimal}/sync"), Route("api/{customerCode}/sync")]
    public class SyncController : BaseController
    {
        private const int LockTimeoutInMs = 1000;

        private IServiceProvider _serviceProvider;
        private ITaskService TaskService => _serviceProvider.GetRequiredService<ITaskService>();

        public SyncController(
            ProductManager manager, 
            ElasticConfiguration configuration, 
            IServiceProvider serviceProvider 
        ) : base(manager, configuration)
        {
            _serviceProvider = serviceProvider;
        }

        [HttpPut]
        [Route("{language}/{state}")]
        public async Task<IActionResult> Put([FromBody] JsonElement message, string language, string state, string instanceId = null)
        {
            if (!ValidateInstance(instanceId, Configuration.DataOptions.InstanceId))
            {
                return CreateUnauthorizedResult(instanceId, Configuration.DataOptions.InstanceId);
            }

            var syncer = Configuration.GetSyncer(language, state);
            var product = message.GetProperty("product");
            var regionTags = Manager.GetRegionTags(message);
            var id = await Manager.GetProductId(product, language, state);
            if (id == null)
            {
                return BadRequest($"Product has no id.");
            }

            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest($"Unable to create/update product {id}. This is read-only instance.");
            }

            Logger.Info("Query received for creating/updating product: {id}", id);

            if (await syncer.EnterSingleCrudAsync(LockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.CreateAsync(product, regionTags, language, state);
                    return CreateResult(result);
                }
                finally
                {
                    syncer.LeaveSingleCrud();
                }
            }
            else
                return BadRequest($"Unable to enter into EnterSingleCRUDAsync during {LockTimeoutInMs} ms");
        }

        [HttpDelete]
        [Route("{language}/{state}")]
        public async Task<object> Delete([FromBody] JsonElement message, string language, string state, string instanceId = null)
        {
            if (!ValidateInstance(instanceId, Configuration.DataOptions.InstanceId))
            {
                return CreateUnauthorizedResult(instanceId, Configuration.DataOptions.InstanceId);
            }

            var syncer = Configuration.GetSyncer(language, state);
            var product = message.GetProperty("product");

            var id = await Manager.GetProductId(product, language, state);
            if (id == null)
            {
                return BadRequest($"Product has no id");
            }
            
            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest($"Unable to remove product {id}. This is read-only instance.");
            }

            Logger.Info("Query received for deleting product: {id}", id);

            if (await syncer.EnterSingleCrudAsync(LockTimeoutInMs))
            {
                try
                {
                    var result = await Manager.DeleteAsync(product, language, state);
                    return CreateResult(result);
                }
                finally
                {
                    syncer.LeaveSingleCrud();
                }
            }
            else
                return BadRequest($"Unable to enter into EnterSingleCRUDAsync during {LockTimeoutInMs} ms");
        }

        [Route("{language}/{state}/reset"), HttpPost]
        public ActionResult Reset(string language, string state)
        {

            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest("Unable to recreate index. This is read-only instance.");
            }

            var syncer = Configuration.GetSyncer(language, state);

            if (!syncer.AnySlotsLeft)
                return BadRequest("There is no available slots. Please, wait for previous operations completing.");

            int taskId = TaskService.AddTask("ReindexAllTask", $"{language}/{state}", 0, null, "ReindexAllTask");
            
            return Json(new { taskId });
        }

        [Route("{language}/{state}/default"), HttpGet]
        public async Task<ActionResult> Default(string language, string state)
        {
            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest("Unable to get default index settings info. This is read-only instance.");
            }
            
            var result = await Manager.GetDefaultIndexSettings(language, state);
            return Content(result, "application/json");
            
        }

        [Route("task"), HttpGet]
        public ActionResult Task(int id)
        {
            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest("Unable to get task info. This is read-only instance.");
            }
            return Json(TaskService.GetTask(id));
        }

        [Route("settings"), HttpGet]
        public ActionResult Settings()
        {
            if (!Configuration.DataOptions.CanUpdate)
            {
                return BadRequest("Unable to get channel info. This is read-only instance.");
            }
            
            var tasks = TaskService.GetTasks(0, int.MaxValue, null, null, null, null, null, null, out _);
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

            var result = JsonSerializer.Serialize(r.ToArray());
            return Content(result, "application/json");
        }

        private IActionResult CreateResult(SonicResult results)
        {
            if (!results.Succeeded)
            {
                LogException(results.GetException(), results.ToString());
                return new ContentResult() { Content = results.ToString(), StatusCode = 500 };
            }

            return new OkResult();
        }

        private IActionResult CreateUnauthorizedResult(string instanceId, string actualInstanceId)
        {
            Logger.Info("InstanceId {instanceId} is specified incorrectly, should be {actualInstanceId}", instanceId, actualInstanceId);            
            return new UnauthorizedResult();
        }

        private bool ValidateInstance(string instanceId, string actualInstanceId)
        {
            return instanceId == actualInstanceId || string.IsNullOrEmpty(instanceId) && string.IsNullOrEmpty(actualInstanceId);
        }   
    }
}
