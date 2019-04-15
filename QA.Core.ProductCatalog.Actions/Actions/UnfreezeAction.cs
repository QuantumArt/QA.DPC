using System.Linq;
using QA.Core.ProductCatalog.Actions.Actions.Abstract;
using QA.ProductCatalog.Infrastructure;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.Core.ProductCatalog.ActionsRunner;
using QA.ProductCatalog.ContentProviders;

namespace QA.Core.ProductCatalog.Actions.Actions
{
    public class UnfreezeAction : ActionTaskBase
    {
        private const string PublishAction = "PublishAction";
        private const string AdapterKey = "Adapter";

        private readonly IFreezeService _freezeService;
        //private readonly Func<string, string, IAction> _getAction;
        private readonly ISettingsService _settingsService;
        private readonly ITaskService _taskService;


        public UnfreezeAction(IFreezeService freezeService, ISettingsService settingsService, ITaskService taskService)
        {
            _freezeService = freezeService;
            _settingsService = settingsService;
            _taskService = taskService;
        }
        public override string Process(ActionContext context)
        {
            var productIds = _freezeService.GetUnfrosenProductIds();

            if (productIds.Any())
            {
                int productContentId = int.Parse(_settingsService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID));

                var publishContext = new ActionContext
                {
                    BackendSid = context.BackendSid,
                    CustomerCode = context.CustomerCode,
                    ContentId = productContentId,
                    ContentItemIds = productIds,
                    Parameters = context.Parameters,
                    UserId = context.UserId,
                    UserName = context.UserName,
                };

                /*
                string actionAdapter = null;
                context.Parameters.TryGetValue(AdapterKey, out actionAdapter);
                var publishAction = _getAction(PublishAction, actionAdapter);
                publishAction.Process(publishContext);
                */

                var dataForQueue = new ActionData
                {
                    ActionContext = publishContext,
                    Description = null,
                    IconUrl = null
                };

                string data = ActionData.Serialize(dataForQueue);

                _taskService.AddTask(PublishAction, data, publishContext.UserId, publishContext.UserName, "Разморозка публикации");

                return "Отправлены на публикацию размороженные продукты: " + string.Join(",", productIds);
            }

            return "Продуктов для разморозки не найдено";
        }
    }
}
