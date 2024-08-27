using System.Text.Json;
using Microsoft.Extensions.Logging;
using Confluent.Kafka;
using Broker.Listener.Shared.Services.Abstracts;
using Broker.Listener.Shared.Models;

namespace Broker.Listener.Shared.Services;

public class KafkaPublisher : IPublisher
{
    private readonly IProducer<Null, byte[]> _publisher;
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public KafkaPublisher(IProducer<Null, byte[]> publisher, ILogger<KafkaPublisher> logger)
    {
        _publisher = publisher;
    }

    public Task SendAsync<T>(T message, Func<Task> onSuccess = null) where T : IngestionMessage
        => SendAsync(message, message.TopicName, onSuccess);

    public Task SendAsync<T>(T message, string topicName, Func<Task> onSuccess = null) where T : IngestionMessage
    {
        var value = JsonSerializer.SerializeToUtf8Bytes(message, s_jsonSerializerOptions);
        if (onSuccess == null)
            _publisher.Produce(topicName, new Message<Null, byte[]>() { Value = value });
        else
        {
            _publisher.Produce(topicName, new Message<Null, byte[]>() { Value = value }
            , async (report) =>
            {
                if (report.Status == PersistenceStatus.Persisted)
                    await onSuccess();
            });
        }
        return Task.CompletedTask;
    }

    public async Task<bool> SendAsync<T>(T message) where T : IngestionMessage
    {
        var topicName = message.TopicName;
        var value = JsonSerializer.SerializeToUtf8Bytes(message, s_jsonSerializerOptions);
        var result = await _publisher.ProduceAsync(topicName, new Message<Null, byte[]>() { Value = value });
        return result.Status == PersistenceStatus.Persisted;
    }
}
