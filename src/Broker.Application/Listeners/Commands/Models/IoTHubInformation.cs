using Newtonsoft.Json;

namespace Broker.Application.Handler.Command.Model
{
    public class IoTHubInformation
    {
        [JsonProperty("iot_hub_name")]
        public string IoTHubName { get; set; }
        [JsonProperty("iot_hub_id")]
        public string IoTHubId { get; set; }
        [JsonProperty("event_hub_name")]
        public string EventHubName { get; set; }
        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; }

    }
}