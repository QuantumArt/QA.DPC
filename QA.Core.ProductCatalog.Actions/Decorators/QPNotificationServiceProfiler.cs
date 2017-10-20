using QA.Core.Models.Entities;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Threading.Tasks;
using QA.Core.Logger;

namespace QA.Core.ProductCatalog.Actions.Decorators
{
	public class QPNotificationServiceProfiler : ProfilerBase, IQPNotificationService
	{
		private readonly IQPNotificationService _notificationService;

		public QPNotificationServiceProfiler(IQPNotificationService notificationService, ILogger logger)
			: base(logger)
		{
			if (notificationService == null)
				throw new ArgumentNullException("notificationService");

			_notificationService = notificationService;
			Service = _notificationService.GetType().Name;
		}

		public async Task<int[]> SendProductsAsync(Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null)
		{
		
				var token = CallMethod("SendProductsAsync", "isStage = {0}", isStage);
				var result = await _notificationService.SendProductsAsync(products, isStage, userName, userId, localize, autopublish, forcedСhannels);
				EndMethod(token);
				return result;
		}

		public int[] SendProducts(Article[] products, bool isStage, string userName, int userId, bool localize, bool autopublish, string[] forcedСhannels = null)
		{
			var token = CallMethod("SendProducts", "isStage = {0}", isStage);
			var result = _notificationService.SendProducts(products, isStage, userName, userId, localize, autopublish, forcedСhannels);
			EndMethod(token);
			return result;
		}

		public Task DeleteProductsAsync(Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null)
		{
			return new Task(() =>
			{
				var token = CallMethod("DeleteProducts(Async)", "ids = {0}", string.Join(",", null));
				//_notificationService.DeleteProductsAsync(ids, userName, userId).RunSynchronously();
				EndMethod(token);
			});
		}

		public Task UnpublishProductsAsync(Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			return new Task(() =>
			{
				var token = CallMethod("UnpublishProducts(Async)", "ids = {0}", string.Join(",", null));
				//_notificationService.UnpublishProductsAsync(ids, userName, userId).RunSynchronously();
				EndMethod(token);
			});
		}

		public void DeleteProducts(Article[] products, string userName, int userId, bool autopublish, string[] forcedСhannels = null)
		{
			var token = CallMethod("DeleteProducts", "ids = {0}", string.Join(",", null));
			//_notificationService.DeleteProducts(ids, userName, userId);
			EndMethod(token);
		}

		public void UnpublishProducts(Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			var token = CallMethod("UnpublishProducts", "ids = {0}", string.Join(",", null));
			//_notificationService.UnpublishProducts(ids, userName, userId);
			EndMethod(token);
		}

		public void DeleteProducts(string[] data, int[] ids, string userName, int userId, string[] forcedСhannels = null)
		{
		}
	}
}
