using System;
using System.Collections.Generic;
using System.Linq;

namespace AHI.Broker.Function.Model.ExportModel
{
    public enum DetailDataType
    {
        @null,
        text,
        number,
        @bool,
        select
    }

    public class BrokerSchema
    {
        private const string BROKER_REST_API = "BROKER_REST_API";
        public Guid Id { get; set; }
        public string Type { get; set; }
        public ICollection<SchemaDetail> Details { get; } = new List<SchemaDetail>();

        public virtual void StandardizeSetting(BrokerModel broker)
        {
            // remove redundant data
            var currentKeys = Details.Select(detail => detail.Key);
            var keysToRemove = broker.Settings.Keys.Where(key => !currentKeys.Contains(key.ToLowerInvariant())).ToArray();
            foreach (var key in keysToRemove)
            {
                broker.Settings.Remove(key);
            }

            // set option name
            var selectTypeDetails = Details.Where(detail => detail.DataType == DetailDataType.select);
            foreach (var detail in selectTypeDetails)
            {
                if (broker.Settings.TryGetValue(detail.Key, out var optionId))
                {
                    var optionName = detail.Options.FirstOrDefault(option => option.Id.Equals(optionId))?.Name;
                    broker.Settings[detail.Key] = optionName;
                }
            }
        }

        public static BrokerSchema CreateSchema(Guid id, string type)
        {
            if (type == BROKER_REST_API)
                return new BrokerRestApiSchema { Id = id, Type = type };

            return new BrokerSchema { Id = id, Type = type };
        }
    }

    public class SchemaDetail
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public DetailDataType DataType { get; set; }
        public ICollection<SchemaOption> Options { get; } = new List<SchemaOption>();
    }

    public class SchemaOption
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class BrokerRestApiSchema : BrokerSchema
    {
        private const string ENDPOINT_INDEX = "endpoint";

        public override void StandardizeSetting(BrokerModel broker)
        {
            broker.Settings.TryGetValue(ENDPOINT_INDEX, out var value);
            base.StandardizeSetting(broker);
            broker.Settings[ENDPOINT_INDEX] = RemoveQueryParam(value as string);
        }

        private string RemoveQueryParam(string url)
        {
            if (url is null)
                return null;

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps)
            {
                return uri.GetLeftPart(UriPartial.Path);
            }
            return string.Empty;
        }
    }
}