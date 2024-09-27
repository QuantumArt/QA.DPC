using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.Models;

namespace QA.Core.DPC.Kafka.API.Controllers
{
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly ILogger<KafkaController> _logger;
        private readonly IKafkaService _kafkaService;
        private readonly KafkaSettings _settings;

        public KafkaController(ILogger<KafkaController> logger, IKafkaService kafkaService, IOptions<KafkaSettings> settings)
        {
            _logger = logger;
            _kafkaService = kafkaService;
            _settings = settings.Value;
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
            _logger.LogInformation("Receive message with customer_code: {code}, product_id: {pid}, state: {state}, language: {lang}, format: {format}, method: {method}",
                customerCode,
                productId,
                state,
                language,
                format,
                Request.Method.ToLowerInvariant());
            
            _logger.LogDebug("Message body: {data}", data);
            
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
                _logger.LogWarning("Send was not successful with message: {message}", result.Message);
                return BadRequest(result.Message);
            }

            _logger.LogInformation("Send was successful with message: {message}", result.Message);
            return Ok();
        }
    }
}