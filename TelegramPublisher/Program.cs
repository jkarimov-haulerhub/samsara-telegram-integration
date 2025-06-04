using Amazon.Extensions.NETCore.Setup;
using TelegramPublisher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDefaultAWSOptions(hostContext.Configuration.GetAWSOptions());
        services.AddAWSService<Amazon.SQS.IAmazonSQS>();
        services.AddHttpClient();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
