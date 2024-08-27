using System;
using System.Collections.Generic;

namespace AHI.Broker.Function.Model.ExportModel
{
    public class BrokerModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string TypeId { get; set; }
        public string Type { get; set; }
        public IDictionary<string, object> Settings { get; set; }
    }
}
