using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AHI.Broker.Function.Constant;

namespace AHI.Broker.Function.Trigger.ServiceBus
{
    public class EventHubListener
    {
        private readonly IMasterService _masterService;
        private readonly ICache _cache;
        private readonly IMemoryCache _memoryCache;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly ILoggerAdapter<EventHubListener> _logger;

        private readonly string _podName;
        private readonly string _brokerId;

        public EventHubListener(
            IConfiguration configuration,
            IMasterService masterService,
            ICache cache,
            IMemoryCache memoryCache,
            IDomainEventDispatcher domainEventDispatcher,
            ILoggerAdapter<EventHubListener> logger)
        {
            _masterService = masterService;
            _cache = cache;
            _memoryCache = memoryCache;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;

            _podName = configuration["PodName"];
            _brokerId = configuration["BrokerId"];
        }

        [FunctionName("EventHubListener")]
        public async Task RunAsync([EventHubTrigger("%EventHubName%", Connection = "EventHubConnection", ConsumerGroup = "%EventHubConsumerGroup%")] string message)
        {
            var hash = message.CalculateMd5Hash();
            var cacheHit = await _cache.GetStringAsync(hash);
            if (!string.IsNullOrEmpty(cacheHit))
            {
                _logger.LogDebug("Cachehit, terminate the request");
                return;
            }
            await _cache.StoreStringAsync(hash, "1", TimeSpan.FromDays(1));
            JObject requestObject = JsonConvert.DeserializeObject<JObject>(message);

            // convert the object include nested object.
            var request = JsonExtension.ParseJObject("", requestObject, parseJArray: false);

            var projectInfos = await GetProjectInfosAsync();

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

        private async Task<IEnumerable<ProjectInfo>> GetProjectInfosAsync()
        {
            var projectInfosKey = string.Format(CacheKeys.EVENTHUB_LISTENER_PROJECT_INFOS_KEY, _brokerId);
            var cacheHitKey = string.Format(CacheKeys.CACHE_HIT_KEY, _podName);

            var cacheHit = await _cache.GetHashByKeyInStringAsync(projectInfosKey, cacheHitKey);

            if (string.IsNullOrEmpty(cacheHit))
            {
                _memoryCache.Set<List<ProjectInfo>>(projectInfosKey, null);
            }

            var projectInfos = _memoryCache.Get<List<ProjectInfo>>(projectInfosKey);
            if (projectInfos == null)
            {
                var allBrokers = await _masterService.GetAllEventHubsBrokersAsync();
                var broker = allBrokers.FirstOrDefault(x => x.Id.ToString() == _brokerId);
                projectInfos = broker?.ProjectInfo.Split(";")
                    .Select(x =>
                    {
                        var pInfo = x.Split('_');
                        return ProjectInfo.Create(pInfo[0], pInfo[1], pInfo[2]);
                    })
                    .ToList() ?? new List<ProjectInfo>();
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
