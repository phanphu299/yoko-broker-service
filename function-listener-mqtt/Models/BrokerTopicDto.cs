using System;

namespace AHI.Broker.Function.Model
{
    public class BrokerTopicDto
    {
        public Guid BrokerId { get; set; }
        public Guid ClientId { get; set; }
        public string AccessToken { get; set; }
        public string Topic { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
