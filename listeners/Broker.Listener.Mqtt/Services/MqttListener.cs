
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MQTTnet.Client;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Broker.Listener.Shared.Constants;
using Broker.Listener.Shared.Exceptions;
using Broker.Listener.Shared.Services;
using Broker.Listener.Shared.Services.Abstracts;
using Broker.Listener.Shared.Extensions;
using Broker.Listener.Shared.Models;


namespace Broker.Listener.Mqtt.Services;
public class MqttListener : BaseListener
{
    private readonly ILogger<MqttListener> _logger;
    private readonly ICache _cache;
    private readonly bool _sendToKafka = true;
    public MqttListener(
        ILogger<MqttListener> logger
        , ICache cache
        , IConfiguration configuration
        , IResourceMonitor resourceMonitor
        , IFuzzyThreadController fuzzyThreadController
        , IDynamicRateLimiter dynamicRateLimiter
        , IPublisher publisher)
    : base(logger, configuration, resourceMonitor, fuzzyThreadController, dynamicRateLimiter, publisher)
    {
        _logger = logger;
        _cache = cache;
        _sendToKafka = configuration.GetValue<bool>("SendToKafka");
    }


    public override async Task HandleMessage(MqttApplicationMessageReceivedEventArgs mqttEvent, MqttClientWrapper wrapper)
    {
        //Check valid topic
        var topicRegex = new Regex(@"(?<projectId>^[a-fA-F0-9-]{36})\/devices\/[^#+$*]+\/telemetry$");
        var match = topicRegex.Match(mqttEvent.ApplicationMessage.Topic);
        if (!match.Success)
            throw new MessageSkippedException("Invalid topic, terminate the request");

        var messageData = mqttEvent.ApplicationMessage.PayloadSegment.Array;
        var hash = messageData.CalculateMd5Hash();
        var cacheHit = await _cache.GetStringAsync(hash);
        if (!string.IsNullOrEmpty(cacheHit))
        {
            await mqttEvent.AcknowledgeAsync(wrapper.TokenSource.Token);
            throw new MessageSkippedException("Cachehit, terminate the request");
        }
        await _cache.StoreStringAsync(hash, "1", TimeSpan.FromDays(1));

        string content = Encoding.UTF8.GetString(messageData);
        //_logger.LogDebug("PAYLOAD --- {content}", content);
        var projectInfos = ConfiguredProjects;
        //get productId and topicName from data.Topic
        string projectId = match.Groups["projectId"].Value;
        var projectInfo = projectInfos.FirstOrDefault(x => x.ProjectId.ToLower() == projectId.ToLower())
            ?? throw new MessageSkippedException("Terminal request if listener dont have any project config or project config does not constains topic projectid");

        if (!_sendToKafka)
        {
            await mqttEvent.AcknowledgeAsync(wrapper.TokenSource.Token);
            return;
        }

        // convert the object include nested object.
        var request = JsonSerializer.Deserialize<Dictionary<string, object>>(mqttEvent.ApplicationMessage.PayloadSegment);
        var message = new IngestionMessage(projectInfo.TenantId, projectInfo.SubscriptionId, projectInfo.ProjectId, request);
        if (!request.ContainsKey(MetricPayloadConstants.DEVICE_ID) || string.IsNullOrEmpty(request[MetricPayloadConstants.DEVICE_ID]?.ToString()))
        {
            message.BrokerType = BrokerTypeConstants.EMQX_MQTT;
        }

        try
        {
            message.TopicName = _useMultiTopic ? $"{_topic}-{Guid.Parse(message.ProjectId):N}" : _topic;
            var persisted = await _publisher.SendAsync(message);
            if (persisted)
            {
                if (mqttEvent != null && wrapper.TokenSource != null)
                {
                    await mqttEvent.AcknowledgeAsync(wrapper.TokenSource.Token);
                }
                else
                {
                    _logger.LogError("********** mqttEvent: {mqttEvent} ----- TokenSource: {tokenSource}", JsonSerializer.Serialize(mqttEvent), JsonSerializer.Serialize(wrapper.TokenSource));
                }
            }
            else
            {
                _logger.LogError($"********** Kafka persist message error **********");
            }
        }
        catch (Exception ex)
        {
            throw new DownstreamDisconnectedException($"********** SendIngestionMessage error: {ex.Message} **********", ex);
        }
    }
}
