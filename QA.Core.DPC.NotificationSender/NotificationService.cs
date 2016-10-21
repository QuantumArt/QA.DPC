using System;
using System.Linq;
using QA.Core.Service.Interaction;
using QA.ProductCatalog.Infrastructure;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;

namespace QA.Core.DPC
{
	public class NotificationService : QAServiceBase, INotificationService
	{
		internal static NotificationSenderConfig _currentConfiguration = null;

		internal static event EventHandler<EventArgs> OnUpdateConfiguration;

		private readonly ILogger _logger;
		private readonly INotificationProvider _provider;
        private INotificationChannelService _channelService;

        public NotificationService()
		{
			_logger = ObjectFactoryBase.Resolve<ILogger>();
			_provider = ObjectFactoryBase.Resolve<INotificationProvider>();
            _channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();
        }
		
		public void PushNotifications(NotificationItem[] notifications, bool isStage, int userId, string userName, string method)
		{
			RunAction(new UserContext(), null, () =>
			{
				Throws.IfArgumentNull(notifications, _ => notifications);

				using (var ctx = new DAL.NotificationsModelDataContext())
				{
					bool needSubmit = false;
					List<NotificationChannel> channels = null;

					if (notifications.Any(n => n.Channels == null))
					{
						channels = _provider.GetConfiguration().Channels;

						if (channels == null)
						{
							var productIds = notifications.Where(n => n.Channels == null).Select(n => n.ProductId);
							_logger.Error("Продукты {0} не поставлены в очередь т.к. остутствуют каналы для публикации" , string.Join(", ", string.Join(", ", productIds)));
							throw new Exception("Не найдено каналов для публикации");
						}
					}

					var messages = new List<DAL.Message>();

					foreach (var notification in notifications)
					{
						if (notification.Channels == null)
						{
							foreach (var channel in channels.Where(c => c.IsStage == isStage))
							{
								if (channel.XPathFilterExpression == null || DocumentMatchesFilter(notification.Data, channel.XPathFilterExpression))
								{
									messages.Add(new DAL.Message
									{
										Created = DateTime.Now,
										Channel = channel.Name,
										Data = notification.Data,
										DataKey = notification.ProductId,
										Method = method,
										UserId = userId,
										UserName = userName
									});
								}
							}
						}
						else
						{
							foreach(var channel in notification.Channels)
							{
								messages.Add(new DAL.Message
								{
									Created = DateTime.Now,
									Channel = channel,
									Data = notification.Data,
									DataKey = notification.ProductId,
									Method = method,
									UserId = userId,
									UserName = userName
								});
							}
						}
					}
				
					if (messages.Any())
					{                     
						ctx.Messages.InsertAllOnSubmit(messages);

						needSubmit = true;

						var productIds = notifications.Select(n => n.ProductId).Distinct();
						_logger.Info("Постановка сообщения {0} для продуктов {1} в очередь, isStage={2}", method, string.Join(", ", productIds), isStage);
					}

					if (needSubmit)
						ctx.SubmitChanges();
				}
			});
		}

        public void UpdateConfiguration()
		{
			if (OnUpdateConfiguration != null)
			{
				OnUpdateConfiguration(this, new EventArgs());
            }
        }

        public ConfigurationInfo GetConfigurationInfo()
        {
            var actualConfiguration = _provider.GetConfiguration();

            var channels = actualConfiguration.Channels.Select(c => c.Name)
                .Union(_currentConfiguration.Channels.Select(c => c.Name))
                .Distinct();

            Dictionary<string, int> countMap;

            using (var ctx = new DAL.NotificationsModelDataContext())
            {
                countMap = ctx.Messages
                    .GroupBy(m => m.Channel)
                    .ToDictionary(g => g.Key, g => g.Count());
            }         

            var chennelsStatistic = _channelService.GetNotificationChannels();

            return new ConfigurationInfo
            {
                Started = DateTime.Now,
                NotificationProvider = _provider.GetType().Name,
                IsAtual = actualConfiguration.Equals(_currentConfiguration),
                Channels = (from channel in channels
                           join s in chennelsStatistic on channel equals s.Name into d
                           from s in d.DefaultIfEmpty()
                           select new ChannelInfo
                           {
                               Name = channel,
                               State = GetState(actualConfiguration.Channels.FirstOrDefault(c => c.Name == channel), _currentConfiguration.Channels.FirstOrDefault(c => c.Name == channel)),
                               Count = countMap.ContainsKey(channel) ? countMap[channel] : 0,
                               LastId = s?.LastId,
                               LastQueued = s?.LastQueued,
                               LastPublished = s?.LastPublished,
                               LastStatus = s?.LastStatus
                           })
                           .ToArray()
                .ToArray()
            };          
        }

        private State GetState(NotificationChannel actual, NotificationChannel current)
        {
            if (actual == null && current == null)
            {
                throw new ArgumentNullException();
            }

            if (actual == null)
            {
                return State.Deleted;
            }
            else if (current == null)
            {
                return State.New;
            }
            else if (current.Equals(actual))
            {
                return State.Actual;
            }
            else
            {
                return State.Chanded;
            }
        }

        private static readonly XPathExpression ProductXPathExpression = XPathExpression.Compile("ProductInfo/Products/Product");

		private bool DocumentMatchesFilter(string xml, XPathExpression xPathFilter)
		{
			var xpathDoc = new XPathDocument(new StringReader(xml));

			var rootProductNavigator = xpathDoc.CreateNavigator().SelectSingleNode(ProductXPathExpression);

			if (rootProductNavigator == null)
				throw new Exception(string.Format("В xml не найден корневой объект по адресу {0}", ProductXPathExpression.Expression));

			var filterResult = rootProductNavigator.Evaluate(xPathFilter);

			return filterResult is bool && (bool)filterResult || filterResult is XPathNodeIterator && ((XPathNodeIterator)filterResult).Count > 0;
		}
	}
}
