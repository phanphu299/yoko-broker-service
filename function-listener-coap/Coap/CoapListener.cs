using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Infrastructure.AzureCoapTriggerExtension;
using AHI.Infrastructure.AzureCoapTriggerExtension.Messaging;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using AHI.Infrastructure.SharedKernel.Abstraction;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace AHI.Broker.Function.Trigger.ServiceBus
{
    public class CoapListener
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IConfiguration _configuration;
        private readonly ILoggerAdapter<CoapListener> _logger;
        private readonly IMemoryCache _memoryCache;
        private const string OLD_TELEMETRY_TOPIC = "$ahi/telemetry";
        public CoapListener(
            IDomainEventDispatcher domainEventDispatcher,
            IConfiguration configuration,
            ILoggerAdapter<CoapListener> logger,
            IMemoryCache memoryCache)
        {
            _domainEventDispatcher = domainEventDispatcher;
            _configuration = configuration;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [FunctionName("CoapListener")]
        public async Task RunAsync(
          [CoapTrigger("%CoapTopicName%")] ICoapMessage data)
        {
            var topicRegex = new Regex(@"(?<projectId>^[a-fA-F0-9-]{36})\/devices\/[^#+$*]+\/telemetry$");
            var wildCardRegex = new Regex("^[+]\\/devices\\/[+]\\/telemetry$");
            var hasProjectIdInTopic = topicRegex.IsMatch(data?.Topic);
            var hasWildCardInTopic = wildCardRegex.IsMatch(data?.Topic);
            var usingOldTopic = string.Equals(OLD_TELEMETRY_TOPIC, data?.Topic);
            _logger.LogInformation("TopicName: {topic}", data?.Topic);

            if (data == null || (!hasProjectIdInTopic && !hasWildCardInTopic && !usingOldTopic))
            {
                _logger.LogDebug("Invalid topic, terminate the request");
                return;
            }

            var messageData = data.GetMessage();
            if (messageData == null)
            {
                _logger.LogDebug("Invalid message, terminate the request");
            }

            string content = Encoding.UTF8.GetString(messageData);
            if (string.IsNullOrEmpty(content))
                return;

            var hash = content.CalculateMd5Hash();
            var cacheHit = _memoryCache.Get<string>(hash);
            if (!string.IsNullOrEmpty(cacheHit))
            {
                _logger.LogDebug("Cachehit, terminate the request");
                return;
            }

            _memoryCache.Set(hash, "1", TimeSpan.FromDays(1));
            JObject requestObject = JsonConvert.DeserializeObject<JObject>(content);
            // convert the object include nested object.
            IDictionary<string, object> request = new Dictionary<string, object>();
            try
            {
                request = JsonExtension.ParseJObject("", requestObject, parseJArray: false);
            }
            catch
            {
                _logger.LogDebug("Parse object fail, terminate the request");
                return;
            }

            _logger.LogTrace($"PROJECT-INFO --- {_configuration["ProjectInfo"]}");
            _logger.LogTrace($"PAYLOAD --- {content}");
            if (_configuration["ProjectInfo"] == null)
            {
                _logger.LogDebug("Project info invalid, terminate the request");
                return;
            }
            var projectInfoStr = _configuration["ProjectInfo"].Split(';');
            IEnumerable<ProjectInfo> projectInfos = projectInfoStr.Where(x => !string.IsNullOrEmpty(x) && x.Split('_').Length >= 3).Select(x =>
            {
                var infoArr = x.Split('_');
                return ProjectInfo.Create(infoArr[0], infoArr[1], infoArr[2]);
            });

            // Project id in topic ex: 34e5ee62-429c-4724-b3d0-3891bd0a08c9/devices/04b26c97-c0e6-4f14-a70f-14e8ca9beb95/telemetry
            _logger.LogDebug($"Coap listener - hasProjectIdInTopic = {hasProjectIdInTopic}");
            if (hasProjectIdInTopic)
            {
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

                    request[MetricPayloadConstants.BROKER_TYPE] = BrokerTypeConstants.EMQX_COAP;
                }

                request[MetricPayloadConstants.TENANT_ID] = projectInfo.TenantId;
                request[MetricPayloadConstants.SUBSCRIPTION_ID] = projectInfo.SubscriptionId;
                request[MetricPayloadConstants.PROJECT_ID] = projectInfo.ProjectId;
                _logger.LogDebug($"Coap listener - request = {JsonConvert.SerializeObject(request)}");
                await _domainEventDispatcher.SendAsync(new IngestionMessage(request, projectInfo));
            }
            else // Wildcard still in topic ex: +/devices/+/telemetry or old devices using topic $ahi/telemetry
            {
                var tasks = new List<Task>();
                foreach (var projectInfo in projectInfos)
                {
                    request["tenantId"] = projectInfo.TenantId;
                    request["subscriptionId"] = projectInfo.SubscriptionId;
                    request["projectId"] = projectInfo.ProjectId;

                    /*
                        *NOTE: IDictionary is a reference type, this action will change its value repeatedly
                        even when it was assigned to any class before, so make sure it's converted to immutable
                        when assigning to any class
                    */

                    tasks.Add(_domainEventDispatcher.SendAsync(new IngestionMessage(request, projectInfo)));
                }
                await Task.WhenAll(tasks);
            }
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
        public IngestionMessage(IDictionary<string, object> payload, AHI.Broker.Function.ProjectInfo projectInfo)
        {
            TenantId = projectInfo.TenantId;
            SubscriptionId = projectInfo.SubscriptionId;
            ProjectId = projectInfo.ProjectId;
            // *IMPORTANT: To prevent its value to be changed outside of the class once assigned
            RawData = payload.ToImmutableDictionary();
        }
    }
}