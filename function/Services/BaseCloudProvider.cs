using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.SharedKernel.Extension;
using Newtonsoft.Json;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Broker.Function.Constant;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.Audit.Service.Abstraction;

namespace AHI.Broker.Function.Service
{
    public abstract class BaseCloudProvider : ICloudProvider
    {
        private readonly ICloudProvider _next;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ITenantContext _tenantContext;
        private readonly INotificationService _notificationService;
        protected readonly IMasterService _masterService;
        public BaseCloudProvider(ICloudProvider next, IHttpClientFactory httpClientFactory, ITenantContext tenantContext, IMasterService masterService, INotificationService notificationService)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _tenantContext = tenantContext;
            _notificationService = notificationService;
            _masterService = masterService;
        }
        public async Task DeployAsync(Guid brokerId)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.BROKER, _tenantContext);
            var brokerPayload = await client.GetByteArrayAsync($"bkr/brokers/{brokerId}");
            var broker = brokerPayload.Deserialize<BrokerDto>();
            if (CanCreate(broker))
            {
                var dto = await CreateAsync(broker);
                dto.Status = "AC";
                await client.PutAsync($"bkr/brokers/{brokerId}", new StringContent(JsonConvert.SerializeObject(dto, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting), System.Text.Encoding.UTF8, "application/json"));
                //send notification
                await _notificationService.SendNotifyAsync("ntf/notifications/broker/notify", new Broker.Function.Model.Notification.BrokerNotificationMessage(NotificationType.BROKER_CHANGE, dto.Id.ToString(), dto));
            }
            else if (_next != null)
            {
                await _next?.DeployAsync(brokerId);
            }
        }
        protected abstract bool CanCreate(BrokerDto broker);
        protected abstract bool CanRemove(BrokerDto broker);
        protected abstract Task<BrokerDto> CreateAsync(BrokerDto broker);
        protected abstract Task RemoveAsync(BrokerDto broker);
        public async Task RemoveAsync(Guid brokerId)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.BROKER, _tenantContext);
            var queryParameters = new NameValueCollection();
            queryParameters.Add("includeDeletedRecords", "true");
            var queryString = string.Join("&", queryParameters.AllKeys.Select(x => $"{x}={queryParameters[x]}"));

            var brokerPayload = await client.GetByteArrayAsync($"bkr/brokers/{brokerId}?{queryString}");
            //var brokerPayload = await client.GetByteArrayAsync($"bkr/brokers/{brokerId}");
            var broker = brokerPayload.Deserialize<BrokerDto>();
            if (CanCreate(broker))
            {
                await RemoveAsync(broker);
            }
            else if (_next != null)
            {
                await _next.RemoveAsync(brokerId);
            }
        }
    }
    internal class BrokerDetail
    {
        public string Tier { get; set; } = "Standard";
        [JsonProperty("throughput_units")]
        public int ThroughputUnit { get; set; }
        [JsonProperty("max_throughput_units")]
        public int MaxThroughputUnit { get; set; }
        [JsonProperty("auto_inflate")]
        public bool AutoInflate { get; set; }

        [JsonProperty("event_hub_name")]
        public string EventHubName { get; set; }
    }
}
