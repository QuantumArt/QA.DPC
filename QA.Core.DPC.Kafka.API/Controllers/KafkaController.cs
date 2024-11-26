using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.Models;
using ILogger = NLog.ILogger;

namespace QA.Core.DPC.Kafka.API.Controllers
{
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IKafkaService _kafkaService;
        private readonly KafkaSettings _settings;

        public KafkaController(
            IKafkaService kafkaService, 
            KafkaSettings settings)
        {
            _kafkaService = kafkaService;
            _settings = settings;
        }

        [HttpPut("{language}/{state}")]
        [HttpDelete("{language}/{state}")]
        public async Task<IActionResult> ProductToKafka([FromBody] string data,
            [FromQuery(Name = "ProductId")] string productId,
            [FromQuery(Name = "customerCode")] string customerCode,
            string state,
            string language,
            CancellationToken cancellationToken,
            [FromQuery(Name = "format")] string format = "json")
        {
            Logger.ForInfoEvent().Message("Message has been received for sending to Kafka")
                .Property("customerCode", customerCode)
                .Property("productId",productId)
                .Property("state", state)
                .Property("language", language)
                .Property("format", format)
                .Property("method", Request.Method.ToLowerInvariant())
                .Log();
            
            Logger.ForTraceEvent().Message("Message body")
                .Property("data", data)
                .Log();
            
            var result = await _kafkaService.SendMessageToKafka(productId,
                _settings.TopicName,
                data,
                customerCode,
                Request.Method.ToLowerInvariant(),
                state,
                language,
                format,
                cancellationToken);

            if (!result.IsSuccess)
            {
                Logger.ForWarnEvent().Message("Message has not been sent to Kafka")
                    .Property("result", result.Message)
                    .Log();
                
                return BadRequest(result.Message);
            }
            
            Logger.ForInfoEvent().Message("Message has been successfully sent to Kafka")
                .Property("result", result.Message)
                .Log();
            
            return Ok();
        }
    }
}