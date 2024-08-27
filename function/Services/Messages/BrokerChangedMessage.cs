using System;
using Newtonsoft.Json;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Enum;
using AHI.Infrastructure.MultiTenancy.Abstraction;

namespace Function.Service.Message
{
    public class BrokerChangedMessage : BusEvent
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public string Type { get; set; }
        [JsonProperty("RequestDeploy")]
        public bool HasConnectionString { get; set; }
        public BrokerChangedMessage()
        {
        }

        public BrokerChangedMessage(Guid id, ITenantContext tenantContext, ActionTypeEnum actionType = ActionTypeEnum.Created)
        {
            Id = id;
            Type = actionType.ToString();
            ActionType = actionType;
            TenantId = tenantContext.TenantId;
            ProjectId = tenantContext.ProjectId;
            SubscriptionId = tenantContext.SubscriptionId;
        }

        public override string TopicName => "broker.application.event.broker.changed";
    }
}
