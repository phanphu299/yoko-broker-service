using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AHI.Infrastructure.AzureMqttTriggerExtension;
using AHI.Infrastructure.AzureMqttTriggerExtension.Messaging;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AHI.Broker.Function.Extension;

namespace AHI.Broker.Function.Trigger.ServiceBus
{
    public class MqttListener
    {
        private const string OLD_TELEMETRY_TOPIC = "$ahi/telemetry";

        private readonly IConfiguration _configuration;
        private readonly IMasterService _masterService;
        private readonly ICache _cache;
        private readonly IMemoryCache _memoryCache;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILoggerAdapter<MqttListener> _logger;

        private readonly string _podName;

        public MqttListener(
            IConfiguration configuration,
            IMasterService masterService,
            ICache cache,
            IMemoryCache memoryCache,
            IDomainEventDispatcher domainEventDispatcher,
            ILoggerAdapter<MqttListener> logger)
        {
            _configuration = configuration;
            _masterService = masterService;
            _cache = cache;
            _memoryCache = memoryCache;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;

            _podName = configuration["PodName"];
        }

        [FunctionName("MqttListener")]
        public async Task RunAsync([MqttTrigger("%MqttTopicName%")] IMqttMessage data)
        {
            _logger.LogDebug("Message received for topic {Topic}.", data.Topic);

            //Check valid topic
            var topicRegex = new Regex(@"(?<projectId>^[a-fA-F0-9-]{36})\/devices\/[^#+$*]+\/telemetry$");
            var validTopic = topicRegex.IsMatch(data?.Topic);
            var usingOldTopic = string.Equals(OLD_TELEMETRY_TOPIC, data?.Topic);
            if (data == null || (!validTopic && !usingOldTopic))
            {
                _logger.LogDebug("Invalid topic, terminate the request");
                return;
            }

            var messageData = data.GetMessage();
            var hash = messageData.CalculateMd5Hash();
            var cacheHit = await _cache.GetStringAsync(hash);
            if (!string.IsNullOrEmpty(cacheHit))
            {
                _logger.LogDebug("Cachehit, terminate the request");
                return;
            }

            string content = Encoding.UTF8.GetString(messageData);
            if (string.IsNullOrEmpty(content))
                return;

            await _cache.StoreStringAsync(hash, "1", TimeSpan.FromDays(1));

            JObject requestObject = JsonConvert.DeserializeObject<JObject>(content);

            // convert the object include nested object.
            var request = JsonExtension.ParseJObject("", requestObject, parseJArray: false);
            _logger.LogTrace($"PAYLOAD --- {content}");

            if (usingOldTopic)
            {
                var projectInfos = GetProjectInfosFromConfiguration();
                var tasks = new List<Task>();
                foreach (var projectInfo in projectInfos)
                {
                    request["tenantId"] = projectInfo.TenantId;
                    request["subscriptionId"] = projectInfo.SubscriptionId;
                    request["projectId"] = projectInfo.ProjectId;

                    tasks.Add(_domainEventDispatcher.SendAsync(new IngestionMessage(request, projectInfo)));
                }
                await Task.WhenAll(tasks);
            }
            else
            {
                var projectInfos = await GetProjectInfosAsync();

                //get productId and topicName from data.Topic
                var match = topicRegex.Match(data.Topic);

                string projectId = match.Groups["projectId"].Value;

                var projectInfo = projectInfos.FirstOrDefault(x => x.ProjectId.ToLower() == projectId.ToLower());

                if (projectInfo == null)
                {
                    _logger.LogDebug("Terminal request if listener dont have any project config or project config does not constains topic projectid");
                    return;
                }

                if (!request.ContainsKey(MetricPayloadConstants.DEVICE_ID) || string.IsNullOrEmpty(request[MetricPayloadConstants.DEVICE_ID]?.ToString()))
                {
                    request[MetricPayloadConstants.TOPIC_NAME] = data.Topic;

                    request[MetricPayloadConstants.BROKER_TYPE] = BrokerTypeConstants.EMQX_MQTT;
                }

                /*
                        *NOTE: IDictionary is a reference type, this action will change its value repeatedly 
                        even when it was assigned to any class before, so make sure it's converted to immutable 
                        when assigning to any class
                */

                request[MetricPayloadConstants.TENANT_ID] = projectInfo.TenantId;
                request[MetricPayloadConstants.SUBSCRIPTION_ID] = projectInfo.SubscriptionId;
                request[MetricPayloadConstants.PROJECT_ID] = projectInfo.ProjectId;
                await _domainEventDispatcher.SendAsync(new IngestionMessage(request, projectInfo));
            }
        }

        private IEnumerable<ProjectInfo> GetProjectInfosFromConfiguration()
        {
            _logger.LogTrace($"PROJECT_INFO --- {_configuration["ProjectInfo"]}");

            var projectInfoStr = _configuration["ProjectInfo"].Split(';');

            List<ProjectInfo> projectInfos = projectInfoStr
                .Where(x => !string.IsNullOrEmpty(x) && x.Split('_').Length >= 3)
                .Select(x =>
                {
                    var infoArr = x.Split('_');
                    return ProjectInfo.Create(infoArr[0], infoArr[1], infoArr[2]);
                })
                .ToList();

            return projectInfos;
        }

        private async Task<IEnumerable<ProjectInfo>> GetProjectInfosAsync()
        {
            var projectInfosKey = CacheKeys.MQTT_LISTENER_PROJECT_INFOS_KEY;
            var cacheHitKey = string.Format(CacheKeys.CACHE_HIT_KEY, _podName);

            var cacheHit = await _cache.GetHashByKeyInStringAsync(projectInfosKey, cacheHitKey);

            if (string.IsNullOrEmpty(cacheHit))
            {
                _memoryCache.Set<List<ProjectInfo>>(projectInfosKey, null);
            }

            var projectInfos = _memoryCache.Get<List<ProjectInfo>>(projectInfosKey);
            if (projectInfos == null)
            {
                var allBrokers = await _masterService.GetAllMqttBrokersAsync();
                projectInfos = allBrokers
                    .DistinctBy(x => x.ProjectId)
                    .Select(x => ProjectInfo.Create(x.TenantId, x.SubscriptionId, x.ProjectId))
                    .ToList();
                _memoryCache.Set(projectInfosKey, projectInfos);
                await _cache.SetHashByKeyAsync(projectInfosKey, cacheHitKey, "1");
            }
            else
            {
                _logger.LogDebug($"{projectInfosKey}/{cacheHitKey} - Cache hit, no change");
            }

            return projectInfos;
        }
    }

    public class IngestionMessage : BusEvent
    {
        public override string TopicName => "ingestion-exchange";
        /// <summary>
        /// Required Field for Kafka
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// Required Field for Kafka
        /// </summary>
        public string SubscriptionId { get; set; }
        /// <summary>
        /// Required Field for Kafka
        /// </summary>
        public string ProjectId { get; set; }
        public IDictionary<string, object> RawData { get; set; }
        public IngestionMessage(IDictionary<string, object> payload, ProjectInfo projectInfo)
        {
            TenantId = projectInfo.TenantId;
            SubscriptionId = projectInfo.SubscriptionId;
            ProjectId = projectInfo.ProjectId;
            // *IMPORTANT: To prevent its value to be changed outside of the class once assigned
            RawData = payload.ToImmutableDictionary();
        }
    }
}
