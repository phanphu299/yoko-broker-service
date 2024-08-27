using System;

namespace AHI.Function.Model
{
    public class EventHubSchedulerInfo
    {
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ProjectId { get; set; }
        public Guid? IntegrationId { get; set; }
        public Guid? BrokerId { get; set; }
    }
}