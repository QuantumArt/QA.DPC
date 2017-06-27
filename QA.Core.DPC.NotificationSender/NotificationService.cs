using System;
using System.Linq;
using QA.Core.Service.Interaction;
using QA.ProductCatalog.Infrastructure;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;
using QA.Core.DPC.QP.Models;
using QA.Core.DPC.DAL;
using QA.Core.DPC.QP.Services;

namespace QA.Core.DPC
{
	public class NotificationService : QAServiceBase, INotificationService
	{
		internal static event EventHandler<string> OnUpdateConfiguration;

        private readonly IIdentityProvider _identityProvider;

        public NotificationService()
		{
            _identityProvider = ObjectFactoryBase.Resolve<IIdentityProvider>();
        }
		
		public void PushNotifications(NotificationItem[] notifications, bool isStage, int userId, string userName, string method, string customerCode)
		{
            _identityProvider.Identity = new Identity(customerCode);

            RunAction(new UserContext(), null, () =>
			{
				Throws.IfArgumentNull(notifications, _ => notifications);
                var logger = ObjectFactoryBase.Resolve<ILogger>();
                var provider = ObjectFactoryBase.Resolve<INotificationProvider>();

                using (var ctx = ObjectFactoryBase.Resolve<NotificationsModelDataContext>())
				{
					bool needSubmit = false;
					List<NotificationChannel> channels = null;

					if (notifications.Any(n => n.Channels == null))
					{
						channels = provider.GetConfiguration().Channels;

						if (channels == null)
						{
							var productIds = notifications.Where(n => n.Channels == null).Select(n => n.ProductId);
							logger.Error("Продукты {0} не поставлены в очередь т.к. остутствуют каналы для публикации" , string.Join(", ", string.Join(", ", productIds)));
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
						logger.Info("Постановка сообщения {0} для продуктов {1} в очередь, isStage={2}", method, string.Join(", ", productIds), isStage);
					}

					if (needSubmit)
						ctx.SubmitChanges();
				}
			});
		}

        public void UpdateConfiguration(string customerCode)
		{
			if (OnUpdateConfiguration != null)
			{
				OnUpdateConfiguration(this, customerCode);
            }
        }

        public ConfigurationInfo GetConfigurationInfo(string customerCode)
        {
            _identityProvider.Identity = new Identity(customerCode);
            var channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();
            var provider = ObjectFactoryBase.Resolve<INotificationProvider>();

            var currentConfiguration = GetCurrentConfiguration(customerCode);
            var actualConfiguration = provider.GetConfiguration();

            var channels = actualConfiguration.Channels.Select(c => c.Name)
                .Union(currentConfiguration.Channels.Select(c => c.Name))
                .Distinct();

            Dictionary<string, int> countMap;

            using (var ctx = ObjectFactoryBase.Resolve<NotificationsModelDataContext>())
            {
                countMap = ctx.Messages
                    .GroupBy(m => m.Channel)
                    .ToDictionary(g => g.Key, g => g.Count());
            }         

            var chennelsStatistic = channelService.GetNotificationChannels();

            return new ConfigurationInfo
            {
                Started = NotificationSender.Started,
                NotificationProvider = provider.GetType().Name,
                IsActual = actualConfiguration.IsEqualTo(currentConfiguration),
                Channels = (from channel in channels
                           join s in chennelsStatistic on channel equals s.Name into d
                           from s in d.DefaultIfEmpty()
                           select new ChannelInfo
                           {
                               Name = channel,
                               State = GetState(actualConfiguration.Channels.FirstOrDefault(c => c.Name == channel), currentConfiguration.Channels.FirstOrDefault(c => c.Name == channel)),
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

        private static NotificationSenderConfig GetCurrentConfiguration(string customerCode)
        {
            return NotificationSender.ConfigDictionary.ContainsKey(customerCode)
                ? NotificationSender.ConfigDictionary[customerCode]
                : NotificationSender.ConfigDictionary[SingleCustomerProvider.Key];
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
            else if (current.IsEqualTo(actual))
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
