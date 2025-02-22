﻿﻿using System;
using System.Linq;
using QA.Core.Service.Interaction;
using QA.ProductCatalog.ContentProviders;
using System.Xml.XPath;
using System.IO;
using System.Collections.Generic;
 using Microsoft.Extensions.Options;
 using QA.Core.DPC.QP.Models;
using QA.Core.DPC.DAL;
using QA.Core.DPC.QP.Services;
 using NLog;
 using NLog.Fluent;

 namespace QA.Core.DPC
{
	public class NotificationService : QAServiceBase, INotificationService
	{
		internal static event EventHandler<string> OnUpdateConfiguration;

        private readonly IIdentityProvider _identityProvider;

        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly IConnectionProvider _connectionProvider;

        public NotificationService(INotificationProvider provider, 
	        IIdentityProvider identityProvider,
	        INotificationChannelService channelService, 
	        IConnectionProvider connectionProvider)
		{
            _identityProvider = identityProvider;
            _connectionProvider = connectionProvider;
		}
		
		public void PushNotifications(NotificationItem[] notifications, bool isStage, int userId, string userName, string method, string customerCode)
		{
            _identityProvider.Identity = new Identity(customerCode);

            RunAction(new UserContext(), null, () =>
			{
				Throws.IfArgumentNull(notifications, _ => notifications);

					List<NotificationChannel> channels = null;
					var provider = ObjectFactoryBase.Resolve<INotificationProvider>();
					if (notifications.Any(n => n.Channels == null))
					{
						channels = provider.GetConfiguration().Channels;

						if (channels == null)
						{
							var productIds = notifications.Where(n => n.Channels == null).Select(n => n.ProductId);
							_logger.Error(
								"Products {0} have not been enqueued because there are no channels for publishing" , 
								string.Join(", ", string.Join(", ", productIds))
							);
							throw new Exception("No channels for publishing");
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
						var ctx = NotificationsModelDataContext.Get(_connectionProvider);
						ctx.Messages.AddRange(messages);

						var productIds = notifications.Select(n => n.ProductId).Distinct();
						_logger.ForInfoEvent()
							.Message("Message {message} for products {productIds} is enqueueing",
								method, string.Join(", ", productIds)
							)
							.Property("isStage", isStage)
							.Log();

						ctx.SaveChanges();
					}
			});
		}

        public void UpdateConfiguration(string customerCode)
		{
            OnUpdateConfiguration?.Invoke(this, customerCode);
        }

        public ConfigurationInfo GetConfigurationInfo(string customerCode)
        {
            _identityProvider.Identity = new Identity(customerCode);
            var provider = ObjectFactoryBase.Resolve<INotificationProvider>();
            var channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();
            var currentConfiguration = GetCurrentConfiguration(customerCode);
            var actualConfiguration = provider.GetConfiguration();

            var channels = actualConfiguration.Channels.Select(c => c.Name)
                .Union(currentConfiguration == null ? new string[0] : currentConfiguration.Channels.Select(c => c.Name))
                .Distinct();

            var ctx = NotificationsModelDataContext.Get(_connectionProvider);
	        var countMap = ctx.Messages
		        .GroupBy(m => m.Channel)
		        .Select(g => new {g.Key, Count = g.Count()})
		        .ToDictionary(g => g.Key, g => g.Count);

            var channelsStatistic = channelService.GetNotificationChannels(customerCode);

            return new ConfigurationInfo
            {
                Started = NotificationSender.Started,
                NotificationProvider = provider.GetType().Name,
                IsActual = actualConfiguration.IsEqualTo(currentConfiguration),
                ActualSettings = new SettingsInfo
                {
                    Autopublish = actualConfiguration.Autopublish,
                    CheckInterval = actualConfiguration.CheckInterval,
                    ErrorCountBeforeWait = actualConfiguration.ErrorCountBeforeWait,
                    PackageSize = actualConfiguration.PackageSize,
                    TimeOut = actualConfiguration.TimeOut,
                    WaitIntervalAfterErrors = actualConfiguration.WaitIntervalAfterErrors
                },
                CurrentSettings = currentConfiguration == null ?
                new SettingsInfo() :
                new SettingsInfo
                {
                    Autopublish = currentConfiguration.Autopublish,
                    CheckInterval = currentConfiguration.CheckInterval,
                    ErrorCountBeforeWait = currentConfiguration.ErrorCountBeforeWait,
                    PackageSize = currentConfiguration.PackageSize,
                    TimeOut = currentConfiguration.TimeOut,
                    WaitIntervalAfterErrors = currentConfiguration.WaitIntervalAfterErrors
                },
                Channels = (from channel in channels
                           join s in channelsStatistic on channel equals s.Name into d
                           from s in d.DefaultIfEmpty()
                           select new ChannelInfo
                           {
                               Name = channel,
                               State = GetState(actualConfiguration.Channels.FirstOrDefault(c => c.Name == channel), currentConfiguration?.Channels.FirstOrDefault(c => c.Name == channel)),
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
            NotificationSenderConfig config;

            if (NotificationSender.ConfigDictionary.TryGetValue(customerCode, out config))
            {
                return config;
            }
            else if (NotificationSender.ConfigDictionary.TryGetValue(SingleCustomerCoreProvider.Key, out config))
            {
                return config;
            }
            else
            {
                return null;
            }
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
				throw new Exception("No root object found in xml by address: " + ProductXPathExpression.Expression);

			var filterResult = rootProductNavigator.Evaluate(xPathFilter);

			return filterResult is bool && (bool)filterResult || filterResult is XPathNodeIterator && ((XPathNodeIterator)filterResult).Count > 0;
		}
	}
}
