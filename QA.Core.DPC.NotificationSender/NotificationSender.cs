using QA.Core.DPC.QP.Models;
using QA.Core.DPC.QP.Services;
using QA.Core.DPC.Service;
using QA.ProductCatalog.ContentProviders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Fluent;

namespace QA.Core.DPC
{
    public partial class NotificationSender : IHostedService
    {
        private const string AutopublishKey = "Autopublish";
        private static string KeySeparator = "#~→";
        private readonly ICustomerProvider _customerProvider;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IIdentityProvider _identityProvider;
        private readonly IFactoryWatcher _configurationWatcher;
        private readonly NotificationProperties _props;
        private readonly static NLog.ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly List<Timer> _senders = new List<Timer>();
        private static readonly Dictionary<string, ChannelState> _lockers = new Dictionary<string, ChannelState>();

        public static ConcurrentDictionary<string, NotificationSenderConfig> ConfigDictionary =
            new ConcurrentDictionary<string, NotificationSenderConfig>();

        public static DateTime Started = DateTime.MinValue;

        public NotificationSender(
            ICustomerProvider customerProvider,
            IConnectionProvider connectionProvider,
            IIdentityProvider identityProvider,
            IFactoryWatcher factoryWatcher,
            IOptions<NotificationProperties> propsAccessor
        )
        {
            _customerProvider = customerProvider;
            _connectionProvider = connectionProvider;
            _identityProvider = identityProvider;
            _configurationWatcher = factoryWatcher;
            _props = propsAccessor.Value;
        }

        public void Start()
        {
            _logger.Info("{serviceName} started",_props.Name);
            Started = DateTime.Now;

            NotificationService.OnUpdateConfiguration += NotificationService_OnUpdateConfiguration;
            _configurationWatcher.OnConfigurationModify += _configurationWatcher_OnConfigurationModify;
            _configurationWatcher.Start();
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

        public void Stop()
        {
            _logger.Info("{serviceName} stopping...", _props.Name);
            NotificationService.OnUpdateConfiguration -= NotificationService_OnUpdateConfiguration;
            _configurationWatcher.OnConfigurationModify -= _configurationWatcher_OnConfigurationModify;
            _configurationWatcher.Stop();

            foreach (var configDictionary in ConfigDictionary.Values)
            {
                foreach (var sender in _senders)
                {
                    sender.Change(
                        new TimeSpan(0, 0, 0, 0, -1), 
                        new TimeSpan(0, 0, configDictionary.CheckInterval)
                    );
                }
            }
            _logger.Info("{serviceName} stopped", _props.Name);
        }

        private void StopConfiguration(string customerCode)
        {
            if (customerCode != SingleCustomerCoreProvider.Key)
            {
                try
                {
                    _logger.Info("start StopConfiguration for {customerCode}", customerCode);
                    var items = _senders.Zip(_lockers.Keys, (s, k) => new {Sender = s, Key = k});
                    var itemsToStop = items.Where(itm => itm.Key.StartsWith(GetKeyPrefix(customerCode)));

                    foreach (var item in itemsToStop)
                    {
                        item.Sender.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                        _logger.Info("Stop sender for {customerCode}", item.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error().Exception(ex)
                        .Message("can not StopConfiguration for {customerCode}", customerCode)
                        .Write();
                }
                finally
                {
                    _logger.Info("end StopConfiguration for {customerCode}", customerCode);
                }
            }
        }

        private void UpdateConfiguration(string customerCode)
        {
            _identityProvider.Identity = new Identity(customerCode);
            var configProvider = ObjectFactoryBase.Resolve<INotificationProvider>();

            try
            {
                string instanceId = _props.InstanceId;
                _logger.Info()
                    .Message("start UpdateConfiguration for {customerCode}", customerCode)
                    .Property("instanceId", instanceId)
                    .Write();

                int delay = 0;
                var items = _senders.Zip(_lockers.Keys, (s, k) => new {Sender = s, Key = k});
                var config = ConfigDictionary.AddOrUpdate(customerCode, code => configProvider.GetConfiguration(),
                    (code, cfg) => configProvider.GetConfiguration());

                foreach (var channel in config.Channels.Where(c => c.DegreeOfParallelism > 0))
                {
                    var key = GetKey(channel.Name, customerCode);

                    if (_lockers.ContainsKey(key))
                    {
                        var sender = items.First(itm => itm.Key == key).Sender;
                        sender.Change(
                            new TimeSpan(0, 0, delay), 
                            new TimeSpan(0, 0, config.CheckInterval)
                        );
                        _logger.Info(
                            "Update sender for {key} whith delay {delay} and interval {interval}",
                            key, delay, config.CheckInterval
                        );
                    }
                    else
                    {
                        var state = new ChannelState {BlockState = null, ErrorsCount = 0};
                        var descriptor = new ChannelDescriptor
                            {ChannelName = channel.Name, CustomerCode = customerCode, InstanceId = instanceId};
                        _lockers.Add(key, state);
                        _senders.Add(new Timer((SendToOneChannel), descriptor, new TimeSpan(0, 0, delay),
                            new TimeSpan(0, 0, config.CheckInterval)));
                        _logger.Info(
                            "Add sender for {key} whith delay {delay} and interval {interval}",
                            key, delay, config.CheckInterval
                        );
                    }

                    delay++;
                }

                if (customerCode != SingleCustomerCoreProvider.Key)
                {
                    var autopublishKey = GetKey(AutopublishKey, customerCode);

                    if (_lockers.ContainsKey(autopublishKey))
                    {
                        var sender = items.First(itm => itm.Key == autopublishKey).Sender;

                        if (config.Autopublish)
                        {
                            sender.Change(
                                new TimeSpan(0, 0, delay),
                                new TimeSpan(0, 0, config.CheckInterval)
                            );
                            _logger.Info(
                                "Update autopublish for {key} whith delay {delay} and interval {interval}",
                                autopublishKey, delay, config.CheckInterval
                            );
                        }
                        else
                        {
                            sender.Change(
                                new TimeSpan(0, 0, 0, 0, -1),
                                new TimeSpan(0, 0, config.CheckInterval)
                            );
                            _logger.Info(
                                "Stop autopublish for {key} whith delay {delay} and interval {interval}",
                                autopublishKey, delay, config.CheckInterval
                            );
                        }
                    }
                    else if (config.Autopublish)
                    {
                        var state = new ChannelState {BlockState = null, ErrorsCount = 0};
                        _lockers.Add(autopublishKey, state);
                        _senders.Add(new Timer(
                            (Autopublish), customerCode,
                            new TimeSpan(0, 0, delay),
                            new TimeSpan(0, 0, config.CheckInterval)
                        ));
                        _logger.Info("Add autopublish for {key} whith delay {delay} and interval {interval}",
                            autopublishKey, delay, config.CheckInterval);
                    }

                    delay++;

                    var itemsToStop = items
                        .Where(itm =>
                            itm.Key.StartsWith(GetKeyPrefix(customerCode)) &&
                            itm.Key != autopublishKey &&
                            !config.Channels.Any(c =>
                                GetKey(c.Name, customerCode) == itm.Key && c.DegreeOfParallelism > 0));

                    foreach (var item in itemsToStop)
                    {
                        item.Sender.Change(
                            new TimeSpan(0, 0, 0, 0, -1),
                            new TimeSpan(0, 0, config.CheckInterval)
                        );
                        _logger.Info("Stop sender for {key} whith delay {delay}", item.Key, delay);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error().Exception(ex)
                    .Message("can not UpdateConfiguration for {customerCode}", customerCode)
                    .Write();
            }
            finally
            {
                _logger.Info("end UpdateConfiguration for {customerCode}", customerCode);
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
            var actualCustomerCode = _connectionProvider.QPMode ? customerCode : SingleCustomerCoreProvider.Key;
            UpdateConfiguration(actualCustomerCode);
        }

        public void SendToOneChannel(object stateInfo)
        {
            var descriptor = (ChannelDescriptor) stateInfo;
            _identityProvider.Identity = new Identity(descriptor.CustomerCode);

            var channelService = ObjectFactoryBase.Resolve<INotificationChannelService>();
            var config = ConfigDictionary[descriptor.CustomerCode];

            try
            {
                var key = GetKey(descriptor.ChannelName, descriptor.CustomerCode);
                var state = _lockers[key];

                if (Monitor.TryEnter(state))
                {
                    _logger.Info().Message("Monitor Enter")
                        .Property("key", key)
                        .Write();

                    try
                    {
                        if (!state.BlockState.HasValue ||
                            state.BlockState.Value.AddSeconds(config.WaitIntervalAfterErrors) <= DateTime.Now)
                        {
                            if (state.BlockState.HasValue)
                            {
                                _logger.Info(
                                    "Temporary channel lock has been released for channel {channel}, customer code {customerCode}",
                                    descriptor.ChannelName, descriptor.CustomerCode
                                );
                                state.BlockState =
                                    null; //снимаем блокировку, если прошел указанные интервал и пробуем отправить снова
                            }

                            var service = ObjectFactoryBase.Resolve<IMessageService>();
                            var res = service.GetMessagesToSend(descriptor.ChannelName, config.PackageSize);

                            if (res.IsSucceeded)
                            {
                                var channel = GetChannel(config, descriptor.ChannelName);
                                var semaphore = new SemaphoreSlim(Math.Max(channel.DegreeOfParallelism, 1));
                                var localState = new ChannelState() {ErrorsCount = 0};
                                var factoryMap = res.Result.Select(m => m.Key).Distinct()
                                    .ToDictionary(k => k, k => new TaskFactory(new OrderedTaskScheduler()));


                                _logger.Info().Message("SendOneMessage Prepre tasks")
                                    .Property("key", key)
                                    .Property("count", res.Result.Count)
                                    .Write();

                                var tasks = res.Result
                                    .Select(m => SendOneMessage(descriptor.CustomerCode, descriptor.InstanceId, config,
                                        channel, service, m, semaphore, factoryMap[m.Key], localState, channelService))
                                    .ToArray();

                                Task.WaitAll(tasks);

                                _logger.Info().Message("SendOneMessage End wait tasks")
                                    .Property("key", key)
                                    .Write();

                                if (localState.ErrorsCount >= config.ErrorCountBeforeWait)
                                {
                                    state.BlockState = DateTime.Now;
                                    _logger.Info(
                                        "Temporary channel lock has been acquired for channel {channel}, customer code {customerCode}",
                                        channel.Name, descriptor.CustomerCode
                                    );
                                }
                            }
                            else
                            {
                                state.BlockState = DateTime.Now;
                                _logger.Info(
                                    "Queue for channel {channel}, customer code {customerCode} is unavailable, temporary lock will be acquired",
                                    descriptor.ChannelName, descriptor.CustomerCode
                                );
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
                    _logger.Info(
                        "Queue for channel {channel}, {customerCode} is busy",
                        descriptor.ChannelName, descriptor.CustomerCode
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.Error().Exception(ex)
                    .Message(
                        "An error occured while processing messages from the queue for channel {channel}, customer code {customerCode}",
                        descriptor.ChannelName, descriptor.CustomerCode
                    )
                    .Write();
            }
        }

        public void Autopublish(object stateInfo)
        {
            var customerCode = stateInfo as string;
            var autopublishKey = GetKey(AutopublishKey, customerCode);
            var config = ConfigDictionary[customerCode];
            var state = _lockers[autopublishKey];

            if (Monitor.TryEnter(state))
            {
                try
                {
                    if (!state.BlockState.HasValue ||
                        state.BlockState.Value.AddSeconds(config.WaitIntervalAfterErrors) <= DateTime.Now)
                    {
                        var task = ObjectFactoryBase.Resolve<ITask>();
                        task.Run(customerCode, null, null, null);
                    }
                }
                catch (Exception ex)
                {
                    state.BlockState = DateTime.Now;
                    _logger.Error().Exception(ex)
                        .Message(
                            "Autopublishing for {customerCode} is unavailable, temporary lock will be acquired.",
                            customerCode
                        )
                        .Write();
                }
                finally
                {
                    Monitor.Exit(state);
                }
            }
            else
            {
                _logger.Info("Autopublishing for {customerCode} is busy", customerCode);
            }
        }

        private async Task SendOneMessage(
            string customerCode,
            string instanceId,
            NotificationSenderConfig config,
            NotificationChannel channel,
            IMessageService service,
            Message message,
            SemaphoreSlim semaphore,
            TaskFactory factory,
            ChannelState state,
            INotificationChannelService channelService)
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
                    _logger.Info().Message("Semaphore before Wait")
                        .Property("customerCode", customerCode)
                        .Property("channel", channel.Name)
                        .Property("currentCount", semaphore.CurrentCount)
                        .Property("productId", message.Key)
                        .Write();

                    semaphore.Wait();
                    _logger.Debug("Start processing message {messageId} ", message.Id);


                    var request = (HttpWebRequest) WebRequest.Create(url);
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

                    using (var httpResponse = (HttpWebResponse) request.GetResponse())
                    {
                        timer.Stop();
                        _logger.Info()
                            .Message(
                                "Message {message} for channel {channel} has been sent on url {url}",
                                message.Method, channel.Name, Uri.UnescapeDataString(url)
                            )
                            .Property("productId", message.Key)
                            .Property("statusCode", httpResponse.StatusCode)
                            .Property("timeTaken", timer.ElapsedMilliseconds)
                            .Property("messageId", message.Id)
                            .Property("customerCode", customerCode)
                            .Write();

                        channelService.UpdateNotificationChannel(customerCode, channel.Name, message.Key,
                            message.Created, httpResponse.StatusCode.ToString());
                    }

                    ;

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
                        _logger.Info()
                            .Message(
                                "Message {message} for channel {channel} has been sent on url {url}",
                                message.Method, channel.Name, Uri.UnescapeDataString(url)
                            )
                            .Property("productId", message.Key)
                            .Property("statusCode", httpResponse.StatusCode)
                            .Property("timeTaken", timer.ElapsedMilliseconds)
                            .Property("messageId", message.Id)
                            .Property("customerCode", customerCode)
                            .Write();

                        channelService.UpdateNotificationChannel(customerCode, channel.Name, message.Key,
                            message.Created, httpResponse.StatusCode.ToString());
                    }
                    else
                    {
                        _logger.Info()
                            .Message(
                                "Message {message} for channel {channel} has not been sent on url {url}",
                                message.Method, channel.Name, Uri.UnescapeDataString(url)
                            )
                            .Property("productId", message.Key)
                            .Property("statusCode", ex.Status)
                            .Property("timeTaken", timer.ElapsedMilliseconds)
                            .Property("messageId", message.Id)
                            .Property("customerCode", customerCode)
                            .Write();

                        channelService.UpdateNotificationChannel(customerCode, channel.Name, message.Key,
                            message.Created, ex.Status.ToString());
                    }

                    _logger.Error().Exception(ex)
                        .Message(
                            "Message {message} for channel {channel} has not been sent on url {url}",
                            message.Method, channel.Name, Uri.UnescapeDataString(url)
                        )
                        .Property("productId", message.Key)
                        .Property("timeTaken", timer.ElapsedMilliseconds)
                        .Property("messageId", message.Id)
                        .Property("customerCode", customerCode)                        
                        .Write();
                }
                finally
                {
                    semaphore.Release();

                    _logger.Info().Message("Semaphore after Release")
                        .Property("customerCode", customerCode)
                        .Property("channel", channel.Name)
                        .Property("currentCount", semaphore.CurrentCount)
                        .Property("productId", message.Key)
                        .Write();
                }
            });
        }

        private static string GetUrl(string customerCode, string instanceId, NotificationChannel channel,
            Message message)
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

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.CompletedTask;
        }
    }
}