using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Service;
using QA.Core.Logger;
using QA.ProductCatalog.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;

namespace QA.Core.DPC
{
    public partial class NotificationSender : ServiceBase
	{
        private const string AutopublishKey = "Autopublish";
		public static ConcurrentDictionary<string, NotificationSenderConfig> ConfigDictionary = new ConcurrentDictionary<string, NotificationSenderConfig>();
	    public static DateTime Started = DateTime.MinValue;
        private static string KeySeparator = "#~→";

        static ICustomerProvider _customerProvider;
        static IConnectionProvider _connectionProvider;
        static IIdentityProvider _identityProvider;
        static IFactoryWatcher _configurationWatcher;

        public ServiceHost serviceHost = null;

		private readonly List<Timer> _senders = new List<Timer>();
		static readonly Dictionary<string, ChannelState> _lockers = new Dictionary<string, ChannelState>();

		public NotificationSender()
		{
		    InitializeComponent();
			UnityConfig.Configure();
            _customerProvider = ObjectFactoryBase.Resolve<ICustomerProvider>();
            _connectionProvider = ObjectFactoryBase.Resolve<IConnectionProvider>();
            _identityProvider = ObjectFactoryBase.Resolve<IIdentityProvider>();
            _configurationWatcher = ObjectFactoryBase.Resolve<IFactoryWatcher>();
        }

		protected override void OnStart(string[] args)
		{
		    Started = DateTime.Now;

            NotificationService.OnUpdateConfiguration += NotificationService_OnUpdateConfiguration;
            _configurationWatcher.OnConfigurationModify += _configurationWatcher_OnConfigurationModify;
            _configurationWatcher.Start();

            if (serviceHost != null)
				serviceHost.Close();

			serviceHost = new ServiceHost(typeof(NotificationService));
			serviceHost.Open();
		}

        private void _configurationWatcher_OnConfigurationModify(object sender, FactoryWatcherEventArgs e)
        {
            foreach (var code in e.DeletedCodes)
            {
                StopConfiguration(code);
            }

            foreach (var code in e.ModifiedCodes)
            {
                UpdateConfiguration(code);
            }

            foreach (var code in e.NewCodes)
            {
                UpdateConfiguration(code);
            }

        }

        protected override void OnStop()
        {            
            NotificationService.OnUpdateConfiguration -= NotificationService_OnUpdateConfiguration;
            _configurationWatcher.OnConfigurationModify -= _configurationWatcher_OnConfigurationModify;
            _configurationWatcher.Stop();


            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }

            foreach (var configDictionary in ConfigDictionary.Values)
            {
                foreach (var sender in _senders)
                {
                    sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, configDictionary.CheckInterval));
                }
            }
        }

        private void StopConfiguration(string customerCode)
        {
            if (customerCode != SingleCustomerProvider.Key)
            {
                var logger = ObjectFactoryBase.Resolve<ILogger>();

                try
                {
                    logger.Info("start StopConfiguration for {0}", customerCode);
                    var items = _senders.Zip(_lockers.Keys, (s, k) => new { Sender = s, Key = k });
                    var itemsToStop = items.Where(itm => itm.Key.StartsWith(GetKeyPrefix(customerCode)));

                    foreach (var item in itemsToStop)
                    {                     
                        item.Sender.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        logger.Info("Stop sender for {0}", item.Key);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorException($"can not StopConfiguration for {customerCode}", ex);
                }
                finally
                {
                    logger.Info("end StopConfiguration for {0}", customerCode);
                }
            }
        }

        private void UpdateConfiguration(string customerCode)
		{
            _identityProvider.Identity = new Identity(customerCode);
            var configProvider = ObjectFactoryBase.Resolve<INotificationProvider>();
            var logger = ObjectFactoryBase.Resolve<ILogger>();
            
            try
            {
                string instanceId = ConfigurationManager.AppSettings["DPC.InstanceId"];
                logger.Info("start UpdateConfiguration for {0}; InstanceId = {1}", customerCode, instanceId);

                int delay = 0;
                var items = _senders.Zip(_lockers.Keys, (s, k) => new { Sender = s, Key = k });
                var config = ConfigDictionary.AddOrUpdate(customerCode, code => configProvider.GetConfiguration(), (code, cfg) => configProvider.GetConfiguration());

                foreach (var channel in config.Channels.Where(c => c.DegreeOfParallelism > 0))
                {
                    var key = GetKey(channel.Name, customerCode);

                    if (_lockers.ContainsKey(key))
                    {
                        var sender = items.First(itm => itm.Key == key).Sender;
                        sender.Change(new TimeSpan(0, 0, delay), new TimeSpan(0, 0, config.CheckInterval));
                        logger.Info("Update sender for {0} whith delay {1} and interval {2}", key, delay, config.CheckInterval);
                    }
                    else
                    {
                        var state = new ChannelState { BlockState = null, ErrorsCount = 0 };
                        var descriptor = new ChannelDescriptor { ChannelName = channel.Name, CustomerCode = customerCode, InstanceId = instanceId };
                        _lockers.Add(key, state);
                        _senders.Add(new Timer((SendToOneChannel), descriptor, new TimeSpan(0, 0, delay), new TimeSpan(0, 0, config.CheckInterval)));
                        logger.Info("Add sender for {0} whith delay {1} and interval {2}", key, delay, config.CheckInterval);
                    }

                    delay++;
                }

                if (customerCode != SingleCustomerProvider.Key)
                {
                    var autopublishKey = GetKey(AutopublishKey, customerCode);

                    if (_lockers.ContainsKey(autopublishKey))
                    {
                        var sender = items.First(itm => itm.Key == autopublishKey).Sender;

                        if (config.Autopublish)
                        {
                            sender.Change(new TimeSpan(0, 0, delay), new TimeSpan(0, 0, config.CheckInterval));
                            logger.Info("Update autopublish for {0} whith delay {1} and interval {2}", autopublishKey, delay, config.CheckInterval);
                        }
                        else
                        {
                            sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, config.CheckInterval));
                            logger.Info("Stop autopublish for {0} whith delay {1} and interval {2}", autopublishKey, delay, config.CheckInterval);
                        }
                    }
                    else if (config.Autopublish)
                    {
                        var state = new ChannelState { BlockState = null, ErrorsCount = 0 };
                        _lockers.Add(autopublishKey, state);
                        _senders.Add(new Timer((Autopublish), customerCode, new TimeSpan(0, 0, delay), new TimeSpan(0, 0, config.CheckInterval)));
                        logger.Info("Add autopublish for {0} whith delay {1} and interval {2}", autopublishKey, delay, config.CheckInterval);
                    }

                    delay++;

                    var itemsToStop = items
                        .Where(itm =>
                            itm.Key.StartsWith(GetKeyPrefix(customerCode)) &&
                            itm.Key != autopublishKey &&
                            !config.Channels.Any(c => GetKey(c.Name, customerCode) == itm.Key && c.DegreeOfParallelism > 0));

                    foreach (var item in itemsToStop)
                    {
                        item.Sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, config.CheckInterval));
                        logger.Info("Stop sender for {0} whith delay {1}", item.Key, delay);
                    }
                }                
            }
            catch (Exception ex)
            {
                logger.ErrorException($"can not UpdateConfiguration for {customerCode}", ex);
            }
            finally
            {
                logger.Info("end UpdateConfiguration for {0}", customerCode);
            }
        }

        private static string GetKeyPrefix(string customerCode)
        {
            return $"{customerCode}_{KeySeparator}";
        }

        private static string GetKey(string channelName, string customerCode)
        {
            return $"{GetKeyPrefix(customerCode)}_{channelName}";
        }
		private void NotificationService_OnUpdateConfiguration(object sender, string customerCode)
		{
            var actualCustomerCode = _connectionProvider.QPMode ? customerCode : SingleCustomerProvider.Key;
            UpdateConfiguration(customerCode);
        }

        public static void SendToOneChannel(object stateInfo)
        {            
            var descriptor = (ChannelDescriptor)stateInfo;
            _identityProvider.Identity = new Identity(descriptor.CustomerCode);
            var logger = ObjectFactoryBase.Resolve<ILogger>();
            var channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();
            var config = ConfigDictionary[descriptor.CustomerCode];

            try
            {                
                var key = GetKey(descriptor.ChannelName, descriptor.CustomerCode);
                var state = _lockers[key];

                if (Monitor.TryEnter(state))
                {
                    try
                    {
                        if (!state.BlockState.HasValue || state.BlockState.Value.AddSeconds(config.WaitIntervalAfterErrors) <= DateTime.Now)
                        {
							if (state.BlockState.HasValue)
                            {
                                logger.Info("Снятие временной блокировки попыток отправки сообщений для канала {0}, кастомер код {1}", descriptor.ChannelName, descriptor.CustomerCode);
								state.BlockState = null; //снимаем блокировку, если прошел указанные интервал и пробуем отправить снова
                            }

                            var service = ObjectFactoryBase.Resolve<IMessageService>();
                            var res = service.GetMessagesToSend(descriptor.ChannelName, config.PackageSize);

                            if (res.IsSucceeded)
                            {
								var channel = GetChannel(config, descriptor.ChannelName);
								var semaphore = new SemaphoreSlim(Math.Max(channel.DegreeOfParallelism, 1));
								var localState = new ChannelState() { ErrorsCount = 0 };
								var factoryMap = res.Result.Select(m => m.Key).Distinct().ToDictionary(k => k, k => new TaskFactory(new OrderedTaskScheduler()));
								var tasks = res.Result
                                    .Select(m => SendOneMessage(descriptor.CustomerCode, descriptor.InstanceId, config, channel, service, m, semaphore, factoryMap[m.Key], localState, channelService, logger))
                                    .ToArray();
                                
								Task.WaitAll(tasks);

								if (localState.ErrorsCount >= config.ErrorCountBeforeWait)
								{
									state.BlockState = DateTime.Now;
                                    logger.Info("Выставление временной блокировки попыток отправки сообщений для канала {0}, кастомер код {1}", channel.Name, descriptor.CustomerCode);
								}
                            }
                            else
                            {
                                state.BlockState = DateTime.Now;
                                logger.LogInfo(() => $"Очередь для канала {descriptor.ChannelName}, кастомер код {descriptor.CustomerCode} недоступна, выставление временной блокировки попыток отправки сообщений");
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
                    logger.Info($"Очередь для канала {0}, кастомер код {1} все еще занята отправкой сообщений", descriptor.ChannelName, descriptor.CustomerCode);
                }
            }           
            catch (Exception ex)
            {
                logger.ErrorException("Ошибка при обработке сообщений из очереди для канала {0}, кастомер код {1}", ex, descriptor.ChannelName, descriptor.CustomerCode);
            }
        }

        public static void Autopublish(object stateInfo)
        {
            var customerCode = stateInfo as string;
            var autopublishKey = GetKey(AutopublishKey, customerCode);
            var config = ConfigDictionary[customerCode];
            var state = _lockers[autopublishKey];

            if (Monitor.TryEnter(state))
            {
                try
                {
                    if (!state.BlockState.HasValue || state.BlockState.Value.AddSeconds(config.WaitIntervalAfterErrors) <= DateTime.Now)
                    {
                        var task = ObjectFactoryBase.Resolve<ITask>();
                        task.Run(customerCode, null, null, null);
                    }
                }
                catch(Exception ex)
                {
                    state.BlockState = DateTime.Now;
                    var logger = ObjectFactoryBase.Resolve<ILogger>();
                    logger.LogInfo(() => $"Автопубликация для кастомер кодя {customerCode} недоступна, выставление временной блокировки попыток отправки сообщений");
                    logger.ErrorException($"Can't run autopublish for {customerCode}", ex);
                }
                finally
                {
                    Monitor.Exit(state);
                }
            }
            else
            {
                var logger = ObjectFactoryBase.Resolve<ILogger>();
                logger.Info("Автопубликация для {0} все еще занята отправкой сообщений", customerCode);
            }
        }

		private static async Task SendOneMessage(
            string customerCode,
            string instanceId,
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
				string url = GetUrl(customerCode, instanceId, channel, message);
			
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

		private static string GetUrl(string customerCode, string instanceId,  NotificationChannel channel, Message message)
		{
            return
                channel.Url +
                "?UserId=" + message.UserId +
                "&UserName=" + Uri.EscapeDataString(message.UserName) +
                "&MsgId=" + message.Id +
                "&ProductId=" + message.Key +
                "&isStage=" + channel.IsStage +
                "&customerCode=" + customerCode +
                "&InstanceId=" + instanceId;
        }

		private static NotificationChannel GetChannel(NotificationSenderConfig config, string channel)
		{
			return config.Channels.FirstOrDefault(x => x.Name == channel);
		}
	}
}
