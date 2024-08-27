using Newtonsoft.Json;

namespace Broker.Application.Handler.Command.Model
{
    public class CoapInformation
    {
        [JsonProperty("topic_name")]
        public string TopicName { get; set; }
        [JsonProperty("user_name")]
        public string UserName { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("qos")]
        public int QoS { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }

    }
}