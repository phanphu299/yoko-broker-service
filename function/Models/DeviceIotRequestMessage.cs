
using System;
using AHI.Broker.Function.Constant;

namespace AHI.Broker.Function.Model
{
    public class DeviceIotRequestMessage
    {
        public string DeviceId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid BrokerId { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string IoTAuthenticationType { get; set; } = DeviceAuthenticationType.SYMMETRIC_KEY;
        public string PrimaryThumbprint { get; set; }
        public string SecondaryThumbprint { get; set; }
        public string BrokerContent { get; set; }
        public string BrokerType { get; set; }
        public string DeviceContent { get; set; }
        public string MessageContent { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
    }
}