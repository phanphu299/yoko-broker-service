namespace AHI.Broker.Function.Model
{
    public class ProjectInfo
    {
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ProjectId { get; set; }
        public static ProjectInfo Create(string tenantId, string subscriptionId, string projectId)
        {
            return new ProjectInfo
            {
                TenantId = tenantId,
                SubscriptionId = subscriptionId,
                ProjectId = projectId
            };
        }
    }
}
