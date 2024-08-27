using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AHI.Broker.Function.Model;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Function.Model;
using AHI.Broker.Function.Service.Abstraction;

namespace Function.Trigger.RabbitMQ
{
    public class DeviceChangedProcessing
    {
        private readonly ICloudDeviceRegistrationProvider _deviceRegistrationService;
        private readonly ITenantContext _tenantContext;

        public DeviceChangedProcessing(ICloudDeviceRegistrationProvider deviceRegistrationService, ITenantContext tenantContext)
        {
            _deviceRegistrationService = deviceRegistrationService;
            _tenantContext = tenantContext;
        }

        [FunctionName("DeviceChangedProcessing")]
        public async Task RunAsync(
        [RabbitMQTrigger("broker.function.device.changed.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger logger)
        {

            BaseModel<DeviceChangedMessage> request = data.Deserialize<BaseModel<DeviceChangedMessage>>();
            var eventMessage = request.Message;
            _tenantContext.SetTenantId(eventMessage.TenantId);
            _tenantContext.SetSubscriptionId(eventMessage.SubscriptionId);
            _tenantContext.SetProjectId(eventMessage.ProjectId);
            if (eventMessage.ActionType == Enum.ActionTypeEnum.Created && eventMessage.BrokerId != null)
            {
                await _deviceRegistrationService.RegisterAsync(eventMessage.BrokerId.Value, eventMessage.BrokerProjectId, eventMessage.Id);
            }
            else if (eventMessage.ActionType == Enum.ActionTypeEnum.Deleted && eventMessage.BrokerId != null)
            {
                await _deviceRegistrationService.UnRegisterAsync(eventMessage.BrokerId.Value, eventMessage.BrokerProjectId, eventMessage.Id);
            }
        }
    }
}
