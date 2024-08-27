using System;

namespace AHI.Broker.Function.Model
{
    public class BrokerTopicDto 
    {
        public Guid ClientId { get; set; }
        public string Topic { get; set; }
    }
}
