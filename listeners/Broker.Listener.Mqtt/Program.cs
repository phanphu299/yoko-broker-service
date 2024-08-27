
using AHI.Infrastructure.Cache.Redis.Extension;
using Broker.Listener.Shared.Models;
using Broker.Listener.Shared.Extensions;
using Broker.Listener.Shared.Services;
using Broker.Listener.Mqtt.Services;

System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddResourceMonitor()
            .AddFuzzyThreadController()
            .AddDynamicRateLimiter();

        var kafkaOption = context.Configuration.GetSection("Kafka").Get<KafkaOption>();
        if (kafkaOption != null && !string.IsNullOrEmpty(kafkaOption.BootstrapServers))
            services.AddKafkaPublisher(kafkaOption);

        services.AddRedisCache();
        services.AddMemoryCache();
        services.AddHostedService<HealthProbeService>();
        services.AddHostedService<MqttListener>();
    })
    .Build();

await host.RunAsync();

partial class Program
{
    public const string SERVICE_NAME = "broker-listener-mqtt-kafka";
}
