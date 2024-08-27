using Function.Enum;

namespace AHI.Function.Model
{
    public class LookupInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LookupTypeCode { get; set; }
        public bool Active { get; set; }

        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public ActionTypeEnum ActionType { get; set; }
    }
}
