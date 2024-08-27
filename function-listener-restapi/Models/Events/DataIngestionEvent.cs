using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;

namespace AHI.Broker.Function.Model.Event
{
    public class DataIngestionEvent : BusEvent
    {
        public override string TopicName => "broker.function.event.file.uploaded";
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public string FilePath { get; set; }

        public DataIngestionEvent(string filePath, ITenantContext tenantContext)
        {
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            FilePath = filePath;
        }
    }
}