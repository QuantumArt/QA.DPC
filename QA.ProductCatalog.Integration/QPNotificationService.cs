using System;
using System.Linq;
using System.Threading.Tasks;
using QA.Core.Service.Interaction;
using QA.ProductCatalog.Infrastructure;
using QA.Core;
using QA.Core.Models.Entities;
using QA.ProductCatalog.Integration.Notifications;
using QA.Core.DPC.Formatters.Services;
using QA.Core.Models;
using QA.Core.Models.UI;

namespace QA.ProductCatalog.Integration
{
	public class QPNotificationService : QAClientBase, IQPNotificationService
	{
		private const string Put = "PUT";
		private const string Delete = "DELETE";
		
		private readonly IContentProvider<NotificationChannel> _channelProvider;
		private readonly Func<string, IArticleFormatter> _getFormatter;
		private readonly ILogger _logger;

		public QPNotificationService(IContentProvider<NotificationChannel> channelProvider, Func<string, IArticleFormatter> getFormatter, ILogger logger)
		{
			_channelProvider = channelProvider;
			_getFormatter = getFormatter;
			_logger = logger;			
		}

		protected override void OnInitializeClient(object service)
		{
		}

		#region IQPNotificationService implementation
		public async Task<int[]> SendProductsAsync(Article[] products, bool isStage, string userName, int userId, string[] forcedСhannels = null)
		{
			return await PushProductsAsync(products, isStage, userName, userId, Put, forcedСhannels);
		}

		public int[] SendProducts(Article[] products, bool isStage, string userName, int userId, string[] forcedСhannels = null)
		{
			return PushProducts(products, isStage, userName, userId, Put, forcedСhannels);
		}

		public async Task DeleteProductsAsync(Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			await PushProductsAsync(products, true, userName, userId, Delete, forcedСhannels);
			await PushProductsAsync(products, false, userName, userId, Delete, forcedСhannels);
		}

		public void DeleteProducts(Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			PushProducts(products, true, userName, userId, Delete, forcedСhannels);
			PushProducts(products, false, userName, userId, Delete, forcedСhannels);
		}

		public async Task UnpublishProductsAsync(Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			await PushProductsAsync(products, false, userName, userId, Delete, forcedСhannels);
		}

		public void UnpublishProducts(Article[] products, string userName, int userId, string[] forcedСhannels = null)
		{
			UnpublishProducts(products, userName, userId, forcedСhannels);
		}
		#endregion

		#region Private methods
		private string Convert(Article product, string formatter, bool isStage)
		{
			return _getFormatter(formatter).Serialize(product, ArticleFilter.DefaultFilter, true);
		}

		private int[] PushProducts(Article[] products, bool isStage, string userName, int userId, string method, string[] forcedСhannels)
		{
			var service = new Notifications.NotificationServiceClient();
			var notifications = GetNotifications(products, isStage, forcedСhannels);

            if (notifications == null)
            {
                return null;
            }
            else if (notifications.Any())
			{
				service.PushNotifications(notifications, isStage, userId, userName, method);
			}

			return notifications.Select(n => n.ProductId).ToArray();
		}

		private async Task<int[]> PushProductsAsync(Article[] products, bool isStage, string userName, int userId, string method, string[] forcedСhannels)
		{
			var service = new Notifications.NotificationServiceClient();
			var notifications = GetNotifications(products, isStage, forcedСhannels);

			if (notifications == null)
			{
				return null;
			}
            else if (notifications.Any())
			{
				await service.PushNotificationsAsync(notifications, isStage, userId, userName, method);
			}

			return notifications.Select(n => n.ProductId).ToArray();
		}

		private NotificationItem[] GetNotifications(Article[] products, bool isStage, string[] forcedСhannels)
		{
			var channels = _channelProvider.GetArticles();
			NotificationItem[] notifications = null;

			if (channels != null && forcedСhannels != null)
			{
				channels = channels.Where(c => forcedСhannels.Contains(c.Name)).ToArray();
			}

			if (channels == null)
			{
				notifications =
					(from p in products
					 select
					new NotificationItem()
					{
						Channels = null,
						Data = Convert(p, typeof(XmlProductFormatter).Name, isStage),
						ProductId = p.Id
					}).ToArray();

			}
			else if (channels.Any())
			{
				channels = channels.Where(c => c.IsStage == isStage).ToArray();

				if (channels.Any())
				{
					notifications = (from c in channels
									 from p in products
									 where Match(c.Filter, p)
									 group new { Channel = c, Product = p } by new { Product = p, c.Formatter } into g
									 select new NotificationItem
									 {
										 Channels = g.Select(x => x.Channel.Name).ToArray(),
										 Data = Convert(g.Key.Product, g.Key.Formatter, isStage),
										 ProductId = g.Key.Product.Id
									 })
									.ToArray();				
				}
			}
			else
			{
				throw new Exception("Не найдено каналов для публикации");
			}

			return notifications;
		}

		private bool Match(string filter, Article product)
		{
			if (string.IsNullOrEmpty(filter))
			{
				return true;
			}
			else
			{
				return FilterableBindingValueProvider.EvaluatePath(filter, product).Any();
			}
		}
		#endregion
	}
}