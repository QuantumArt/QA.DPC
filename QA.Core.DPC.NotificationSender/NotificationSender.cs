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

namespace QA.Core.DPC
{
	public partial class NotificationSender : ServiceBase
	{
		static NotificationSenderConfig _config;
		static INotificationProvider _configProvider;
        static INotificationChannelService _channelService;
        static ILogger _logger;

	public ServiceHost serviceHost = null;

		private readonly List<Timer> _senders = new List<Timer>();
		static readonly Dictionary<string, ChannelState> _lockers = new Dictionary<string, ChannelState>();

		public NotificationSender()
		{
		InitializeComponent();

			UnityConfig.Configure();

			_logger = ObjectFactoryBase.Resolve<ILogger>();
			_configProvider = ObjectFactoryBase.Resolve<INotificationProvider>();
            _channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();

        }

		protected override void OnStart(string[] args)
		{
			UpdateConfiguration();
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

            foreach (var sender in _senders)
            {
                sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, _config.CheckInterval));
            }
        }

		private void UpdateConfiguration()
		{
			int delay = 0;
			var items = _senders.Zip(_lockers.Keys, (s, c) => new { Sender = s, Channel = c });
			_config = _configProvider.GetConfiguration();
			NotificationService._currentConfiguration = _config;

			foreach (var channel in _config.Channels.Where(c => c.DegreeOfParallelism > 0))
			{
				if (_lockers.ContainsKey(channel.Name))
				{
					var sender = items.First(itm => itm.Channel == channel.Name).Sender;
					sender.Change(new TimeSpan(0, 0, delay), new TimeSpan(0, 0, _config.CheckInterval));
                    _logger.Info("Updete sender for {0} whith delay {1} and interval {2}", channel.Name, delay, _config.CheckInterval);

                }
				else
				{
					_lockers.Add(channel.Name, new ChannelState { BlockState = null, ErrorsCount = 0 });
					_senders.Add(new Timer((SendToOneChannel), channel.Name, new TimeSpan(0, 0, delay), new TimeSpan(0, 0, _config.CheckInterval)));
                    _logger.Info("Add sender for {0} whith delay {1} and interval {2}", channel.Name, delay, _config.CheckInterval);
                }

				delay++;
			}

			var itemsToStop = items
				.Where(itm => !_config.Channels.Any(c => c.Name == itm.Channel && c.DegreeOfParallelism > 0));

			foreach (var item in itemsToStop)
			{
				item.Sender.Change(new TimeSpan(0, 0, 0, 0, -1), new TimeSpan(0, 0, _config.CheckInterval));
                _logger.Info("Stop sender for {0}", item.Channel, delay);
            }
		}
		private void NotificationService_OnUpdateConfiguration(object sender, EventArgs e)
		{
			UpdateConfiguration();
		}
		public static void SendToOneChannel(Object stateInfo)
        {
			string channelName = (string)stateInfo;

            try
            {
				var state = _lockers[channelName];

				if (Monitor.TryEnter(state))
                {
                    try
                    {
						if (!state.BlockState.HasValue || state.BlockState.Value.AddSeconds(_config.WaitIntervalAfterErrors) <= DateTime.Now)
                        {
							if (state.BlockState.HasValue)
                            {
                                _logger.Info("Снятие временной блокировки попыток отправки сообщений для канала {0}", channelName);
								state.BlockState = null; //снимаем блокировку, если прошел указанные интервал и пробуем отправить снова
                            }

							IMessageService service = ObjectFactoryBase.Resolve<IMessageService>();

							var res = service.GetMessagesToSend(channelName, _config.PackageSize);
                            if (res.IsSucceeded)
                            {
								var channel = GetChannel(channelName);
								var semaphore = new SemaphoreSlim(Math.Max(channel.DegreeOfParallelism, 1));
								var localState = new ChannelState() { ErrorsCount = 0 };
								var factoryMap = res.Result.Select(m => m.Key).Distinct().ToDictionary(k => k, k => new TaskFactory(new OrderedTaskScheduler()));
								var tasks = res.Result.Select(m => SendOneMessage(channel, service, m, semaphore, factoryMap[m.Key], localState)).ToArray();
                                
								Task.WaitAll(tasks);

								if (localState.ErrorsCount >= _config.ErrorCountBeforeWait)
								{
									state.BlockState = DateTime.Now;
									_logger.Info("Выставление временной блокировки попыток отправки сообщений для канала {0}", channel.Name);
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
					_logger.Info("Очередь для канала {0} все еще занята отправкой сообщений", channelName);
                }
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Ошибка при обработке сообщений из очереди для канала {0}", ex, channelName);
            }
        }

		private static async Task SendOneMessage(NotificationChannel channel, IMessageService service, Message message, SemaphoreSlim semaphore, TaskFactory factory, ChannelState state)
		{
			await factory.StartNew(() =>
			{				
				lock (state)
				{
					if (state.ErrorsCount >= _config.ErrorCountBeforeWait)
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
					_logger.LogDebug(() => "Начало обработки сообщения MsgId = " + message.Id);

					
					var request = (HttpWebRequest)WebRequest.Create(url);
					request.Method = message.Method.ToUpper();
					request.Timeout = 1000 * _config.TimeOut;
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
						_logger.Info(
						    "Отправлено сообщение {1} для канала {0} по адресу {2}, ProductId = {3}, StatusCode = {4}, MsgId = {5}, TimeTaken = {6}",
						    channel.Name,
						    message.Method,
						    Uri.UnescapeDataString(url),
						    message.Key,
						    httpResponse.StatusCode,
						    message.Id,
						    timer.ElapsedMilliseconds);
                        _channelService.UpdateNotificationChannel(channel.Name, message.Key, message.Created, httpResponse.StatusCode.ToString());
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
						_logger.Info(
							"Отправлено сообщение {1} для канала {0} по адресу {2}, ProductId = {3}, StatusCode = {4}, MsgId = {5}, TimeTaken = {6}",
							channel.Name,
							message.Method,
							Uri.UnescapeDataString(url),
							message.Key,
							httpResponse.StatusCode,
							message.Id,
							timer.ElapsedMilliseconds);
                        _channelService.UpdateNotificationChannel(channel.Name, message.Key, message.Created, httpResponse.StatusCode.ToString());
                    }
					else
					{
						_logger.Info(
							"Не отправлено сообщение {1} для канала {0} по адресу {2} по причине {3}, ProductId = {4}, MsgId = {5}, TimeTaken = {6}",
							channel.Name,
							message.Method,
							Uri.UnescapeDataString(url),
							ex.Status,
							message.Key,
							message.Id,
							timer.ElapsedMilliseconds);
                        _channelService.UpdateNotificationChannel(channel.Name, message.Key, message.Created, ex.Status.ToString());
                    }

					_logger.ErrorException(
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

		private static NotificationChannel GetChannel(string channel)
		{
			return _config.Channels.FirstOrDefault(x => x.Name == channel);
		}
	}
}
