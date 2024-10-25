using System.Text;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using QA.Core.DPC.Kafka.Helpers;
using QA.Core.DPC.Kafka.Models;
using ILogger = NLog.ILogger;

namespace QA.Core.DPC.Kafka.API.Controllers
{
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        private readonly KafkaSettings _settings;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger(); 
        public HealthCheckController(KafkaSettings settings)
        {
            _settings = settings;
        }
        
        [HttpGet]
        public ActionResult Index()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Application: OK");
            var brokerOkStr = IsKafkaConnected() ? "OK" : "Error";
            sb.AppendLine("Broker: " + brokerOkStr);
            return Content(sb.ToString(), "text/plain");
        }

        private bool IsKafkaConnected()
        {
            var adminClient = new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = _settings.Producer.BootstrapServers
                }).SetLogHandler((_, message) =>
                {
                    KafkaHelper.LogSysLogMessage(Logger, message);
                }).Build();
            
            var timeout = TimeSpan.FromMilliseconds(_settings.RequestTimeoutInMs);
            
            try
            {
                _ = adminClient.GetMetadata(timeout);
                return true;
            }
            catch (KafkaException kex)
            {
                Logger.ForErrorEvent().Exception(kex).Message("Cannot connect to Kafka").Log();
                return false;
            }
        }
    }
}