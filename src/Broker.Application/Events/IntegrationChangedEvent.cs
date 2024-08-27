using System;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Enum;

namespace Broker.Application.Event
{
    public class IntegrationChangedEvent : BusEvent
    {
        public override string TopicName => "broker.application.event.integration.changed";
        public Guid Id { get; }
        public string Name { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public string Type { get; set; }
        public bool RequestDeploy { get; set; }
        public IntegrationChangedEvent(Guid id, string name, string type, ITenantContext tenantContext, bool requestDeploy, ActionTypeEnum actionType = ActionTypeEnum.Updated)
        {
            Id = id;
            Name = name;
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            ActionType = actionType;
            Type = type;
            RequestDeploy = requestDeploy;
        }
    }
}
