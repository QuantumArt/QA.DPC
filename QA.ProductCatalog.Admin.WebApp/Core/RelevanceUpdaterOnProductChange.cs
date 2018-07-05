using System.Collections.Generic;
using QA.Core;
using QA.Core.ProductCatalog.Actions;
using QA.Core.ProductCatalog.Actions.Actions;
using QA.Core.ProductCatalog.ActionsRunnerModel;
using QA.Core.ProductCatalog.Actions.Tasks;
using QA.ProductCatalog.Infrastructure;

namespace QA.ProductCatalog.Admin.WebApp.Core
{
    public class RelevanceUpdaterOnProductChange : IProductChangeSubscriber
	{
		public void NotifyProductsChanged(Dictionary<int, int[]> affectedProductIdsByContentId)
		{
			var settingService = ObjectFactoryBase.Resolve<ISettingsService>();

			int productContentId = int.Parse(settingService.GetSetting(SettingsTitles.PRODUCTS_CONTENT_ID));

			if (!affectedProductIdsByContentId.ContainsKey(productContentId))
				return;

			var taskService = ObjectFactoryBase.Resolve<ITaskService>();

			const int userId = 1;

			string taskData =
				ActionData.Serialize(new ActionData
				{
					ActionContext =
						new ActionContext { ContentItemIds = affectedProductIdsByContentId[productContentId], ContentId = productContentId, Parameters = new Dictionary<string, string>() }
				});

			var taskKey = typeof(ProductRelevanceAction).Name;

			taskService.AddTask(taskKey, taskData, userId, "Админ", "Обновление статусов на витрине");
		}
	}
}