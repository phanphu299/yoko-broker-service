using Newtonsoft.Json;

namespace Broker.Application.Handler.Command.Model
{
    public class WaylayInformation
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        [JsonProperty("broker_endpoint")]
        public string BrokerEndpoint { get; set; }
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
        [JsonProperty("api_secret")]
        public string ApiSecret { get; set; }

        [JsonProperty("pooling_interval")]
        public int Interval { get; set; }
    }
}