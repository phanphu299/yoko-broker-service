using System.Collections.Generic;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;

namespace Function.Service.Model
{
    public class IngestionMessage : BusEvent
    {
        public override string TopicName => "ingestion-exchange";
        public IDictionary<string, object> RawData { get; set; }
        public IngestionMessage(IDictionary<string, object> payload)
        {
            RawData = payload;
        }
    }
}