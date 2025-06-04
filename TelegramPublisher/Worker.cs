using System.Collections.Generic;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;
using TelegramPublisher.Models;

namespace TelegramPublisher;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IAmazonSQS _sqs;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _queueUrl;
    private readonly string _telegramToken;
    private readonly string _chatId;

    public Worker(ILogger<Worker> logger, IAmazonSQS sqs, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _sqs = sqs;
        _httpClientFactory = httpClientFactory;
        _queueUrl = config.GetValue<string>("Sqs:QueueUrl") ?? throw new InvalidOperationException("QueueUrl missing");
        _telegramToken = config.GetValue<string>("Telegram:BotToken") ?? throw new InvalidOperationException("BotToken missing");
        _chatId = config.GetValue<string>("Telegram:ChatId") ?? throw new InvalidOperationException("ChatId missing");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = _queueUrl,
                    MaxNumberOfMessages = 3,
                    WaitTimeSeconds = 10
                }, stoppingToken);

                foreach (var message in response.Messages)
                {
                    await ProcessMessageAsync(message, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling SQS");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken ct)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<SamsaraEvent>(message.Body);
            if (payload != null)
            {
                var text = $"\uD83D\uDEA8 Event: {payload.EventType} by Driver {payload.DriverName} at {payload.EventTime}";
                var client = _httpClientFactory.CreateClient();
                var url = $"https://api.telegram.org/bot{_telegramToken}/sendMessage";
                var response = await client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["chat_id"] = _chatId,
                    ["text"] = text
                }), ct);
                response.EnsureSuccessStatusCode();
            }
            await _sqs.DeleteMessageAsync(_queueUrl, message.ReceiptHandle, ct);
            _logger.LogInformation("Processed message {Id}", message.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message");
        }
    }
}
