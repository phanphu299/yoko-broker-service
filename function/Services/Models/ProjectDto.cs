using System;


namespace AHI.Broker.Function.Service.Model
{
    public class ProjectDto
    {
        public string Id => ResourceId;
        public string SubscriptionId => SubscriptionResourceId;
        public string TenantId => TenantResourceId;
        public string ResourceId { get; set; }
        public string SubscriptionResourceId { get; set; }
        public string TenantResourceId { get; set; }
        public string TenantName { get; set; }
        public string Email { get; set; }
        public string SubscriptionName { get; set; }
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public string ProjectType { get; set; }
        public bool IsMigrated { get; set; }
        public bool Deleted { get; set; }
    }
}