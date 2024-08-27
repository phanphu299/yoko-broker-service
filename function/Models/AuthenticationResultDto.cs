using Newtonsoft.Json;

namespace AHI.Broker.Function.Model
{
    public class AuthenticationResultDto
    {
        public string Result { get; set; } = "deny";
        [JsonProperty("is_superuser")]
        public bool IsSuperUser { get; set; } = false;
    }
}
