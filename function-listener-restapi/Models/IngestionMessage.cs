using System.Collections.Generic;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;

namespace AHI.Broker.Function.Model
{
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
        public IngestionMessage(IDictionary<string, object> payload, ITenantContext tenantContext)
        {
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            RawData = payload;
        }
    }
}