using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Broker.Function.Constant;

namespace AHI.Broker.Function.Service
{
    public abstract class BaseCloudDeviceRegistrationProvider : ICloudDeviceRegistrationProvider
    {
        private protected ICloudDeviceRegistrationProvider _next;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly ITenantContext _tenantContext;
        public BaseCloudDeviceRegistrationProvider(ICloudDeviceRegistrationProvider next, IHttpClientFactory httpClientFactory, ITenantContext tenantContext)
        {
            _next = next;
            _httpClientFactory = httpClientFactory;
            _tenantContext = tenantContext;
        }
        public async Task RegisterAsync(Guid brokerId, string projectId, string deviceId)
        {
            var tenantContext = _tenantContext.Clone();
            tenantContext.SetProjectId(projectId ?? _tenantContext.ProjectId);
            var client = _httpClientFactory.CreateClient(HttpClientNames.BROKER, tenantContext);
            var brokerPayload = await client.GetByteArrayAsync($"bkr/brokers/{brokerId}");
            var broker = brokerPayload.Deserialize<BrokerDto>();
            if (CanRegister(broker))
            {
                var dto = await RegisterAsync(broker, deviceId);
                dto.Status = "AC";
                dto.DeviceCount = await GetTotalDeviceAsync(brokerId);
                await client.PutAsync($"bkr/brokers/{brokerId}", new StringContent(JsonConvert.SerializeObject(dto, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting), System.Text.Encoding.UTF8, "application/json"));
                //send notification
                //await _notificationService.SendNotifyAsync("ntf/notifications/broker/notify", new Broker.Function.Model.Notification.NotificationMessage(NotificationType.BROKER_CHANGE, null, dto.Id.ToString(), dto));
            }
            else if (_next != null)
            {
                await _next.RegisterAsync(brokerId, projectId, deviceId);
            }
        }

        private Task<int> GetTotalDeviceAsync(Guid brokerId)
        {
            return Task.FromResult<int>(1);
        }

        public async Task UnRegisterAsync(Guid brokerId, string projectId, string deviceId)
        {
            var tenantContext = _tenantContext.Clone();
            tenantContext.SetProjectId(projectId ?? _tenantContext.ProjectId);
            var client = _httpClientFactory.CreateClient(HttpClientNames.BROKER, tenantContext);
            var brokerPayload = await client.GetByteArrayAsync($"bkr/brokers/{brokerId}");
            var broker = brokerPayload.Deserialize<BrokerDto>();
            if (CanRegister(broker))
            {
                var dto = await UnRegisterAsync(broker, deviceId);
                dto.Status = "AC";
                dto.DeviceCount = await GetTotalDeviceAsync(brokerId);
                await client.PutAsync($"bkr/brokers/{brokerId}", new StringContent(JsonConvert.SerializeObject(dto, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting), System.Text.Encoding.UTF8, "application/json"));
                //send notification
                //await _notificationService.SendNotifyAsync("ntf/notifications/broker/notify", new Broker.Function.Model.Notification.NotificationMessage(NotificationType.BROKER_CHANGE, null, dto.Id.ToString(), dto));
            }
            else if (_next != null)
            {
                await _next.UnRegisterAsync(brokerId, projectId, deviceId);
            }
        }
        protected abstract bool CanRegister(BrokerDto broker);
        protected abstract Task<BrokerDto> RegisterAsync(BrokerDto broker, string deviceId);
        protected abstract Task<BrokerDto> UnRegisterAsync(BrokerDto broker, string deviceId);
    }
}