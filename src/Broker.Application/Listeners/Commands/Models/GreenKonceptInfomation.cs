using Newtonsoft.Json;

namespace Broker.Application.Handler.Command.Model
{
    public class GreenKonceptInfomation
    {

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
        [JsonProperty("client_id")]
        public string ClientId { get; set; }
        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }
}
