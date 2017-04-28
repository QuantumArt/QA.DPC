using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QA.Core.DPC.Service;
using System.Threading.Tasks.Schedulers;
using System.Diagnostics;
using QA.ProductCatalog.Infrastructure;
using System.Collections.Concurrent;
using QA.Core.DPC.QP.Servives;
using QA.Core.DPC.QP.Models;

namespace QA.Core.DPC
{
	public partial class NotificationSender : ServiceBase
	{
		public static ConcurrentDictionary<string, NotificationSenderConfig> _configDictionary = new ConcurrentDictionary<string, NotificationSenderConfig>();
        static ICustomerProvider _customerProvider;
        static IIdentityProvider _identityProvider;

        public ServiceHost serviceHost = null;

		private readonly List<Timer> _senders = new List<Timer>();
		static readonly Dictionary<string, ChannelState> _lockers = new Dictionary<string, ChannelState>();

		public NotificationSender()
		{
		    InitializeComponent();
			UnityConfig.Configure();
            _customerProvider = ObjectFactoryBase.Resolve<ICustomerProvider>();
            _identityProvider = ObjectFactoryBase.Resolve<IIdentityProvider>();
        }

		protected override void OnStart(string[] args)
		{
            foreach (var customer in _customerProvider.GetCustomers())
            {
                UpdateConfiguration(customer.CustomerCode);
            }

			NotificationService.OnUpdateConfiguration += NotificationService_OnUpdateConfiguration;

			if (serviceHost != null)
				serviceHost.Close();

			serviceHost = new ServiceHost(typeof(NotificationService));
			serviceHost.Open();
		}

		protected override void OnStop()
        {
			NotificationService.OnUpdateConfiguration -= NotificationService_OnUpdateConfiguration;

			if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }

            foreach (var configDictionary in _configDictionary.Values)
            {
                foreach (var sender in _senders)
                {
                    sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, configDictionary.CheckInterval));
                }
            }
        }

		private void UpdateConfiguration(string customerCode)
		{
            _identityProvider.Identity = new Identity(customerCode);
            var configProvider = ObjectFactoryBase.Resolve<INotificationProvider>();
            var logger = ObjectFactoryBase.Resolve<ILogger>();
            int delay = 0;
			var items = _senders.Zip(_lockers.Keys, (s, k) => new { Sender = s, Key = k });
            var config = _configDictionary.AddOrUpdate(customerCode, code => configProvider.GetConfiguration(), (code, cfg) => configProvider.GetConfiguration());

			foreach (var channel in config.Channels.Where(c => c.DegreeOfParallelism > 0))
			{
                var key = GetKey(channel.Name, customerCode);

                if (_lockers.ContainsKey(key))
				{
					var sender = items.First(itm => itm.Key == key).Sender;
					sender.Change(new TimeSpan(0, 0, delay), new TimeSpan(0, 0, config.CheckInterval));
                    logger.Info("Updete sender for {0} whith delay {1} and interval {2}", key, delay, config.CheckInterval);
                }
				else
				{
                    var state = new ChannelState { BlockState = null, ErrorsCount = 0 };
                    var descriptor = new ChannelDescriptor { ChannelName = channel.Name, CustomerCode = customerCode };
                    _lockers.Add(key, state);
					_senders.Add(new Timer((SendToOneChannel), descriptor, new TimeSpan(0, 0, delay), new TimeSpan(0, 0, config.CheckInterval)));
                    logger.Info("Add sender for {0} whith delay {1} and interval {2}", key, delay, config.CheckInterval);
                }

				delay++;
			}

			var itemsToStop = items
				.Where(itm => !config.Channels.Any(c => GetKey(c.Name, customerCode) == itm.Key && c.DegreeOfParallelism > 0));

			foreach (var item in itemsToStop)
			{
				item.Sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, config.CheckInterval));
                logger.Info("Stop sender for {0} whith delay {1}", item.Key, delay);
            }
		}

        private static string GetKey(string channelName, string customerCode)
        {
            return $"{customerCode}_{channelName}";
        }
		private void NotificationService_OnUpdateConfiguration(object sender, string customerCode)
		{
			UpdateConfiguration(customerCode);
		}

		public static void SendToOneChannel(Object stateInfo)
        {            
            var descriptor = (ChannelDescriptor)stateInfo;
            _identityProvider.Identity = new Identity(descriptor.CustomerCode);
            var logger = ObjectFactoryBase.Resolve<ILogger>();
            var channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();
            var config = _configDictionary[descriptor.CustomerCode];

            try
            {
                var key = GetKey(descriptor.ChannelName, descriptor.CustomerCode);
                var state = _lockers[key];

				if (Monitor.TryEnter(state))
                {
                    try
                    {
						if (!state.BlockState.HasValue || state.BlockState.Value.AddSeconds(config.WaitIntervalAfterErrors) >= DateTime.Now)
                        {
							if (state.BlockState.HasValue)
                            {
                                logger.Info("Снятие временной блокировки попыток отправки сообщений для канала {0}, кастомер код {1}", descriptor.ChannelName, descriptor.CustomerCode);
								state.BlockState = null; //снимаем блокировку, если прошел указанные интервал и пробуем отправить снова
                            }

							IMessageService service = ObjectFactoryBase.Resolve<IMessageService>();

							var res = service.GetMessagesToSend(descriptor.ChannelName, config.PackageSize);
                            if (res.IsSucceeded)
                            {
								var channel = GetChannel(config, descriptor.ChannelName);
								var semaphore = new SemaphoreSlim(Math.Max(channel.DegreeOfParallelism, 1));
								var localState = new ChannelState() { ErrorsCount = 0 };
								var factoryMap = res.Result.Select(m => m.Key).Distinct().ToDictionary(k => k, k => new TaskFactory(new OrderedTaskScheduler()));
								var tasks = res.Result
                                    .Select(m => SendOneMessage(config, channel, service, m, semaphore, factoryMap[m.Key], localState, channelService, logger))
                                    .ToArray();
                                
								Task.WaitAll(tasks);

								if (localState.ErrorsCount >= config.ErrorCountBeforeWait)
								{
									state.BlockState = DateTime.Now;
                                    logger.Info("Выставление временной блокировки попыток отправки сообщений для канала {0}, кастомер код {1}", channel.Name, descriptor.CustomerCode);
								}
                            }
                        }
                    }
                    finally
                    {
						Monitor.Exit(state);
                    }
                }
                else
                {
                    logger.Info("Очередь для канала {0}, кастомер код {1} все еще занята отправкой сообщений", descriptor.ChannelName, descriptor.CustomerCode);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorException("Ошибка при обработке сообщений из очереди для канала {0}, кастомер код {1}", ex, descriptor.ChannelName, descriptor.CustomerCode);
            }
        }

		private static async Task SendOneMessage(
            NotificationSenderConfig config,
            NotificationChannel channel,
            IMessageService service,
            Message message,
            SemaphoreSlim semaphore,
            TaskFactory factory,
            ChannelState state,
            INotificationChannelService channelService,
            ILogger logger)
		{
			await factory.StartNew(() =>
			{				
				lock (state)
				{
					if (state.ErrorsCount >= config.ErrorCountBeforeWait)
					{
						return;
					}
				}

				var timer = new Stopwatch();
				timer.Start();
				string url = GetUrl(channel, message);
			
				try
				{
					semaphore.Wait();
                    logger.LogDebug(() => "Начало обработки сообщения MsgId = " + message.Id);

					
					var request = (HttpWebRequest)WebRequest.Create(url);
					request.Method = message.Method.ToUpper();
					request.Timeout = 1000 * config.TimeOut;
				    var mediaType = !string.IsNullOrEmpty(channel.MediaType) ? channel.MediaType : "text/xml";
                    request.ContentType = $"{mediaType}; charset=utf-8";
                    byte[] data = Encoding.UTF8.GetBytes(message.Xml);
					request.ContentLength = data.Length;

					using (var streamWriter = request.GetRequestStream())
					{
						streamWriter.Write(data, 0, data.Length);
						streamWriter.Flush();
					}

					using (var httpResponse = (HttpWebResponse)request.GetResponse())
					{
						timer.Stop();
                        logger.Info(
						    "Отправлено сообщение {1} для канала {0} по адресу {2}, ProductId = {3}, StatusCode = {4}, MsgId = {5}, TimeTaken = {6}",
						    channel.Name,
						    message.Method,
						    Uri.UnescapeDataString(url),
						    message.Key,
						    httpResponse.StatusCode,
						    message.Id,
						    timer.ElapsedMilliseconds);
                        channelService.UpdateNotificationChannel(channel.Name, message.Key, message.Created, httpResponse.StatusCode.ToString());
                    };
					
					service.RemoveMessage(message.Id);
				}
				catch (WebException ex)
				{
					timer.Stop();

					lock (state)
					{
						state.ErrorsCount++;
					}

					var httpResponse = ex.Response as HttpWebResponse;

					if (httpResponse != null)
					{
                        logger.Info(
							"Отправлено сообщение {1} для канала {0} по адресу {2}, ProductId = {3}, StatusCode = {4}, MsgId = {5}, TimeTaken = {6}",
							channel.Name,
							message.Method,
							Uri.UnescapeDataString(url),
							message.Key,
							httpResponse.StatusCode,
							message.Id,
							timer.ElapsedMilliseconds);
                        channelService.UpdateNotificationChannel(channel.Name, message.Key, message.Created, httpResponse.StatusCode.ToString());
                    }
					else
					{
                        logger.Info(
							"Не отправлено сообщение {1} для канала {0} по адресу {2} по причине {3}, ProductId = {4}, MsgId = {5}, TimeTaken = {6}",
							channel.Name,
							message.Method,
							Uri.UnescapeDataString(url),
							ex.Status,
							message.Key,
							message.Id,
							timer.ElapsedMilliseconds);
                        channelService.UpdateNotificationChannel(channel.Name, message.Key, message.Created, ex.Status.ToString());
                    }

                    logger.ErrorException(
						"Ошибка при отправке сообщения {0} для канала {1} по адресу {2}, ProductId = {3}, MsgId = {4}, TimeTaken = {5}",
						ex, message.Method,
						channel.Name,
						Uri.UnescapeDataString(url),
						message.Key,
						message.Id,
						timer.ElapsedMilliseconds);

				}
				finally
				{
					semaphore.Release();
				}
			});
		}

		private static string GetUrl(NotificationChannel channel, Message message)
		{
			return
				channel.Url +
				"?UserId=" + message.UserId +
				"&UserName=" + Uri.EscapeDataString(message.UserName) +
				"&MsgId=" + message.Id +
				"&ProductId=" + message.Key +
				"&isStage=" + channel.IsStage;
		}

		private static NotificationChannel GetChannel(NotificationSenderConfig config, string channel)
		{
			return config.Channels.FirstOrDefault(x => x.Name == channel);
		}
	}
}
