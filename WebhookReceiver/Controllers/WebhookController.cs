using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebhookReceiver.Models;

namespace WebhookReceiver.Controllers
{
    [ApiController]
    [Route("api/webhooks/[controller]")]
    public class SamsaraController : ControllerBase
    {
        private readonly IAmazonSQS _sqs;
        private readonly ILogger<SamsaraController> _logger;
        private readonly string _queueUrl;

        public SamsaraController(IAmazonSQS sqs, IConfiguration config, ILogger<SamsaraController> logger)
        {
            _sqs = sqs;
            _logger = logger;
            _queueUrl = config.GetValue<string>("Sqs:QueueUrl") ?? throw new InvalidOperationException("QueueUrl missing");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JsonElement payload)
        {
            try
            {
                var messageBody = payload.GetRawText();
                await _sqs.SendMessageAsync(new SendMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MessageBody = messageBody
                });
                _logger.LogInformation("Queued Samsara event");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue webhook payload");
                return StatusCode(500, "Error queuing webhook");
            }
        }
    }
}
