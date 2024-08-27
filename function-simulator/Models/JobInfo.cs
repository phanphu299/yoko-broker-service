using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AHI.Broker.Function.Models
{
    public class JobInfo
    {
        public string FileName { get; set; }
        public BrokerInfo Broker { get; set; }
        public int Interval { get; set; }
        public IList<string> Lines { get; set; }
        public ProjectInfo Project { get; set; }
        public string DeviceId { get; set; }
        public int CurrentLine { get; set; }
    }
    public class BrokerInfo
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public string Content { get; set; }
        public string Type { get; set; } = "BROKER_EVENT_HUB";
    }
    public class BrokerContent
    {
        [JsonProperty("connection_string")]
        public string ConnectionString { get; set; }
        [JsonProperty("event_hub_name")]
        public string EventHubName { get; set; }
    }
    public class ProjectInfo
    {
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}