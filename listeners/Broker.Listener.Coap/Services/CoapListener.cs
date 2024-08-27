using System.Text;
using System.Text.RegularExpressions;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Broker.Listener.Shared.Constants;
using Broker.Listener.Shared.Exceptions;
using Broker.Listener.Shared.Services;
using Broker.Listener.Shared.Services.Abstracts;
using Broker.Listener.Shared.Models;


namespace Broker.Listener.Coap.Services;

public class CoapListener : BaseListener
{
    private readonly ILogger<CoapListener> _logger;
    private readonly ICache _cache;


    public CoapListener(
        ILogger<CoapListener> logger
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
    }


    public override async Task HandleMessage(MqttApplicationMessageReceivedEventArgs e, MqttClientWrapper wrapper)
    {
        var topic = e.ApplicationMessage.Topic ?? "";
        _logger.LogInformation("TopicName: {topic}", topic);

        var topicRegex = new Regex(@"(?<projectId>^[a-fA-F0-9-]{36})\/devices\/[^#+$*]+\/telemetry$");
        var wildCardRegex = new Regex("^[+]\\/devices\\/[+]\\/telemetry$");
        var hasProjectIdInTopic = topicRegex.IsMatch(topic);
        var hasWildCardInTopic = wildCardRegex.IsMatch(topic);
        if (!hasProjectIdInTopic && !hasWildCardInTopic)
            throw new MessageSkippedException("Invalid topic, terminate the request");

        var messageData = e.ApplicationMessage.PayloadSegment.Array ?? throw new MessageSkippedException("Invalid message, terminate the request");
        var content = Encoding.UTF8.GetString(messageData);
        var hash = content.CalculateMd5Hash();
        var cacheHit = await _cache.GetStringAsync(hash);
        if (!string.IsNullOrEmpty(cacheHit))
            throw new MessageSkippedException("Cachehit, terminate the request");

        await _cache.StoreStringAsync(hash, "1", TimeSpan.FromDays(1));
        JObject requestObject = JsonConvert.DeserializeObject<JObject>(content);
        // convert the object include nested object.
        IDictionary<string, object> request = new Dictionary<string, object>();
        try
        {
            request = JsonExtension.ParseJObject("", requestObject, parseJArray: false);
        }
        catch
        {
            throw new MessageSkippedException("Parse object fail, terminate the request");
        }

        _logger.LogTrace("PAYLOAD --- {content}", content);
        var projectInfos = ConfiguredProjects;
        if (!projectInfos.Any())
            throw new MessageSkippedException("Project info invalid, terminate the request");

        // Project id in topic ex: 34e5ee62-429c-4724-b3d0-3891bd0a08c9/devices/04b26c97-c0e6-4f14-a70f-14e8ca9beb95/telemetry
        _logger.LogDebug("Coap listener - hasProjectIdInTopic = {hasProjectIdInTopic}", hasProjectIdInTopic);
        if (hasProjectIdInTopic)
        {
            //get productId and topicName from topic
            var match = topicRegex.Match(topic);

            string projectId = match.Groups["projectId"].Value;

            var projectInfo = projectInfos.FirstOrDefault(x => x.ProjectId.ToLower() == projectId.ToLower());

            if (projectInfo == null)
            {
                _logger.LogDebug("Terminal request if listener dont have any project config or project config does not constains topic projectid");
                return;
            }

            var message = new IngestionMessage(projectInfo.TenantId, projectInfo.SubscriptionId, projectInfo.ProjectId, request);
            request.TryGetValue(MetricPayloadConstants.DEVICE_ID, out object deviceId);
            if (string.IsNullOrEmpty(deviceId?.ToString()))
            {
                message.BrokerType = BrokerTypeConstants.EMQX_COAP;
            }
            _logger.LogDebug($"Coap listener - request = {JsonConvert.SerializeObject(message)}");
            await SendIngestionMessage(message, onSuccess: async () => await e.AcknowledgeAsync(wrapper.TokenSource.Token));
        }
        else // Wildcard still in topic ex: +/devices/+/telemetry
        {
            var tasks = new List<Task>();
            foreach (var projectInfo in projectInfos)
            {
                /*
                    *NOTE: IDictionary is a reference type, this action will change its value repeatedly
                    even when it was assigned to any class before, so make sure it's converted to immutable
                    when assigning to any class
                */
                var message = new IngestionMessage(projectInfo.TenantId, projectInfo.SubscriptionId, projectInfo.ProjectId, request);
                tasks.Add(SendIngestionMessage(message, onSuccess: async () => await e.AcknowledgeAsync(wrapper.TokenSource.Token)));
            }
            await Task.WhenAll(tasks);
        }
    }
}
