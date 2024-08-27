using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Confluent.Kafka;

using Broker.Listener.Shared.Constants;
using Broker.Listener.Shared.Models;
using Broker.Listener.Shared.Services;
using Broker.Listener.Shared.Services.Abstracts;

namespace Broker.Listener.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFuzzyThreadController(this IServiceCollection services)
    {
        return services.AddSingleton<IFuzzyThreadController, FuzzyThreadController>();
    }

    public static IServiceCollection AddResourceMonitor(this IServiceCollection services)
    {
        return services.AddSingleton<IResourceMonitor, ResourceMonitor>();
    }

    public static IServiceCollection AddDynamicRateLimiter(this IServiceCollection services)
    {
        return services.AddSingleton<IDynamicRateLimiter, DynamicRateLimiter>();
    }

    public static IServiceCollection AddKafkaPublisher(this IServiceCollection services, KafkaOption kafkaOption)
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetService<IConfiguration>();
            var bootstrapServers = kafkaOption.BootstrapServers;
            if (string.IsNullOrEmpty(bootstrapServers))
                return null;

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = kafkaOption.AckMode ?? Acks.Leader,
                LingerMs = kafkaOption.Linger ?? LingerConstants.LINGGER_MS,
                BatchSize = kafkaOption.BatchSize
            };
            return new ProducerBuilder<Null, byte[]>(config).Build();
        });
        services.AddSingleton<IPublisher, KafkaPublisher>();
        return services;
    }
}
