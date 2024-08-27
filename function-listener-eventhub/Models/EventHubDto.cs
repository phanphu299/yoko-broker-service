using System;

namespace AHI.Broker.Function.Model
{
    public class EventHubDto
    {
        public Guid Id { get; set; }
        public string ConnectionString { get; set; }
        public string HubName { get; set; }
        public string Type { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectInfo { get; set; }
        public string Status { get; set; }
    }
}
