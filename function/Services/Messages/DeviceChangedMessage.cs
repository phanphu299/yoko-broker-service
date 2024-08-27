using System;
using Function.Enum;

namespace Function.Service.Message
{
    public class DeviceChangedMessage
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public bool IsRetentionDayChanged { get; set; }
        public ActionTypeEnum ActionType { get; set; }
        public Guid? BrokerId { get; set; }
    }
}