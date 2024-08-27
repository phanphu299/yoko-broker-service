using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using AHI.Broker.Function.Model.ImportModel.Converter;

namespace AHI.Broker.Function.Model.ImportModel
{
    public class BrokerModel : FileParser.Model.ImportModel
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        [JsonConverter(typeof(JsonStringTrimmer))]
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringTrimmer))]
        public string Type { get; set; }
        [JsonIgnore]
        public string Status { get; set; } = "IN";
        public IDictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        [JsonIgnore]
        public bool ShouldReplace { get; set; }
    }
}
