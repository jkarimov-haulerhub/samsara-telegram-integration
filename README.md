# Samsara Telegram Integration

This repository contains two .NET 7 applications:

1. **WebhookReceiver** – ASP.NET Core Web API that receives Samsara webhook events and queues them to AWS SQS.
2. **TelegramPublisher** – Background worker that reads events from SQS and posts formatted messages to a Telegram chat.

## Building

```bash
# Restore and build all projects
export PATH=$HOME/.dotnet:$PATH
dotnet restore SamsaraTelegramIntegration.sln
dotnet build SamsaraTelegramIntegration.sln
```

Both applications read configuration from their `appsettings.json` files. Update AWS credentials, queue URL, and Telegram bot settings before running.
