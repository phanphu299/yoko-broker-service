using System;
using Newtonsoft.Json;

namespace AHI.Broker.Function.Model
{
    public abstract class PayloadObject
    {
        [JsonProperty(Order = -3)]
        public Guid ActivityId { get; set; }
        [JsonProperty(Order = -3)]
        public string ObjectName { get; set; }
        [JsonProperty(Order = -2)]
        public virtual string Status { get; set; }
        [JsonProperty(Order = -2)]
        public DateTime CreatedUtc { get; set; }
        [JsonProperty(Order = -2)]
        public string Description { get; set; }
        public void CopyTo(PayloadObject target)
        {
            target.ActivityId = ActivityId;
            target.ObjectName = ObjectName;
            target.CreatedUtc = CreatedUtc;
            target.Description = Description;
        }
    }

    public class NotifyPayloadObject : PayloadObject
    {
        [JsonProperty(Order = -2)]
        public string NotifyType { get; set; }
    }
}