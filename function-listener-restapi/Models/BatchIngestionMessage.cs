using System;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;

namespace AHI.Broker.Function.Model
{
    public class BatchIngestionMessage : BusEvent
    {
        public override string TopicName => "broker.function.event.ingestion.batch";
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ProjectId { get; set; }
        public Guid BrokerId { get; set; }
        public byte[] RawData { get; set; }

        public BatchIngestionMessage()
        {
        }

        public BatchIngestionMessage(string tenantId, string subscriptionId, string projectId, Guid brokerId, byte[] data)
        {
            TenantId = tenantId;
            SubscriptionId = subscriptionId;
            ProjectId = projectId;
            BrokerId = brokerId;
            RawData = data;
        }
    }
}