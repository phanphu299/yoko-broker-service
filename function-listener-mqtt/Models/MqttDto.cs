using System;
using System.Collections.Generic;

namespace AHI.Broker.Function.Model
{
    public class MqttDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<BrokerTopicDto> Topics { get; set; }
        public string UserName { get; set; }
        public string AccessToken { get; set; }
        public int QoS { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public string ProjectId { get; set; }
    }
}
