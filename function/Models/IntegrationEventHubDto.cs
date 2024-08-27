using System;
using Newtonsoft.Json;

namespace AHI.Function.Model
{
    public class IntegrationEventHubDto
    {
        public Guid Id { set; get; }
        public string Name { set; get; }
        public string Type { set; get; }
        public string Content { set; get; }
    }
    public class IntegrationEventHubInformationDto
    {
        [JsonProperty(PropertyName = "connection_string")]
        public string ConnectionString { set; get; }
        [JsonProperty(PropertyName = "event_hub_name")]
        public string EventHubName { set; get; }
    }
}
