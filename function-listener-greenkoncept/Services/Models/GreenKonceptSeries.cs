
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Function.Service.Model
{
    public class GreenKonceptSeries
    {
        public string NodeId { get; set; }
        public IEnumerable<GreenKonceptSeriesEvent> Events { get; set; }
    }

    public class GreenKonceptSeriesEvent
    {
        public IEnumerable<GreenKonceptSeriesEventData> EventData { get; set; }
        public long Timestamp { get; set; }
    }
    public class GreenKonceptSeriesEventData
    {
        public long Value { get; set; }
        public string measure { get; set; }
    }

    public class GreenKonceptSeriesPayload
    {
        [JsonProperty("nodeUID")]
        public string NodeUID { get; set; }
        [JsonProperty("measures")]
        public IEnumerable<string> Measures { get; set; }
    }
}
