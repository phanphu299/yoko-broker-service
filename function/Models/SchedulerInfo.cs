using System;

namespace AHI.Function.Model
{
    public class SchedulerInfo
    {
        public Guid SchedulerId { get; set; }
        public Guid IntegrationId { get; set; }
        public string DeviceId { get; set; }
        public string ProjectId { get; set; }
        public DateTime LastRunDate { get; set; }
        public int PoolingInterval { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string Endpoint { get; set; }
        public string AuthorizationKey { get; set; }
        public string AuthorizationType { get; set; }
        public string Type { get; set; }
    }
}