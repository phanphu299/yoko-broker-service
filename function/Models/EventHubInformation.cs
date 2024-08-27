using Newtonsoft.Json;

namespace AHI.Function.Model
{
    public class EventHubInformation
    {
        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; }
        [JsonProperty("event_hub_name")]
        public string EventHubName { get; set; }
        [JsonProperty("event_hub_id")]
        public string EventHubId { get; set; }

    }
}