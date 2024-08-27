using System;

namespace AHI.Broker.Function.Model
{
    public class BrokerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}