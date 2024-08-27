using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Function.Service.Message;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Enum;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;

namespace Function.Trigger.RabbitMQ
{
    public class BrokerChangedProcessing
    {
        private readonly ICloudProvider _cloudProvider;
        private readonly ITenantContext _tenantContext;
        private readonly IDomainEventDispatcher _dispatcher;

        public BrokerChangedProcessing(ICloudProvider cloudProvider, ITenantContext tenantContext, IDomainEventDispatcher dispatcher)
        {
            _cloudProvider = cloudProvider;
            _tenantContext = tenantContext;
            _dispatcher = dispatcher;
        }

        [FunctionName("BrokerChangedProcessing")]
        public async Task RunAsync(
        [RabbitMQTrigger("broker.function.broker.changed.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger log)
        {
            BaseModel<BrokerChangedMessage> request = data.Deserialize<BaseModel<BrokerChangedMessage>>();
            if (request != null)
            {
                // insert into tenant device
                var eventMessage = request.Message;
                _tenantContext.SetTenantId(eventMessage.TenantId);
                _tenantContext.SetSubscriptionId(eventMessage.SubscriptionId);
                _tenantContext.SetProjectId(eventMessage.ProjectId);

                switch (eventMessage.ActionType)
                {
                    case ActionTypeEnum.Created:
                    case ActionTypeEnum.Updated:
                        if (eventMessage.HasConnectionString)
                            return;

                        await _cloudProvider.DeployAsync(eventMessage.Id);
                        break;

                    case ActionTypeEnum.Deleted:
                        await _cloudProvider.RemoveAsync(eventMessage.Id);
                        break;
                }

                var listenerEvent = new BrokerListenerChangedMessage()
                {
                    Id = eventMessage.Id,
                    ActionType = eventMessage.ActionType,
                    HasConnectionString = eventMessage.HasConnectionString,
                    ProjectId = eventMessage.ProjectId,
                    SubscriptionId = eventMessage.SubscriptionId,
                    TenantId = eventMessage.TenantId,
                    Type = eventMessage.Type
                };
                await _dispatcher.SendAsync(listenerEvent);
            }
        }
    }
}
