namespace AHI.Broker.Function.Model
{
    public class CleanupRequestMessage
    {
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
    }
}
