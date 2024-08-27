using System;
using System.Collections.Generic;

namespace AHI.Function.Model
{
    public class BrokerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public string Status { get; set; } = "IN";
        public int DeviceCount { get; set; }
        public IDictionary<string, object> Details { get; set; }
    }
}
