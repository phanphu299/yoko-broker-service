using System;
using Function.Enum;

namespace AHI.Broker.Function.Model
{
    public class DeviceChangedMessage
    {
        public string Id { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public Guid? BrokerId { get; set; }
        public string BrokerProjectId { get; set; }
        public ActionTypeEnum ActionType { get; set; }
    }
}