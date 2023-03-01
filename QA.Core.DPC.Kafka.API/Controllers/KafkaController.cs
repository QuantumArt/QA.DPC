using Microsoft.AspNetCore.Mvc;
using QA.Core.DPC.Kafka.API.Interfaces;
using QA.Core.DPC.Kafka.API.Models;

namespace QA.Core.DPC.Kafka.API.Controllers
{
    [Route("api/[controller]")]
    public class KafkaController : ControllerBase
    {
        private readonly ILogger<KafkaController> _logger;
        private readonly IKafkaService _kafkaService;

        public KafkaController(ILogger<KafkaController> logger, IKafkaService kafkaService)
        {
            _logger = logger;
            _kafkaService = kafkaService;
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
            _logger.LogInformation("Receive message with customer_code: {code}, product_id: {pid}, state: {state}, language: {lang}, format: {format}, method: {method} and body: {data}",
                customerCode,
                productId,
                state,
                language,
                format,
                Request.Method.ToLowerInvariant(),
                data);
            
            SendResult result = await _kafkaService.SendMessageToKafka(productId,
                data,
                customerCode,
                Request.Method,
                state,
                language,
                format,
                cancellationToken);
            
            _logger.LogInformation("Send was successful: {success} with message {message}",
                result.IsSuccess,
                result.Message);

            return result.IsSuccess ? Ok() : BadRequest(result.Message);
        }
    }
}