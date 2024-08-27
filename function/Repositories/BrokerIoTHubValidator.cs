using System;
using System.Collections.Generic;
using System.Linq;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.Model.ImportModel;

namespace AHI.Infrastructure.Repository
{
    public class BrokerIoTHubValidator : BaseBrokerPersistenceValidator
    {
        private static readonly string[] IntegerDataKeys = new[] { "number_of_hub_units", "device_to_cloud_partitions" };
        private static readonly IDictionary<string, object> DefaultValues = new Dictionary<string, object>
        {
            { "number_of_hub_units", 2 },
            { "defender_for_iot", false },
            { "device_to_cloud_partitions", 4 }
        };

        public BrokerIoTHubValidator(IDictionary<string, IImportTrackingService> errorHandlers) : base(errorHandlers)
        {
        }

        public override bool CanApply(string brokerType) => brokerType == BrokerType.BROKER_IOT_HUB;

        public override void ProcessPreValidate(BrokerModel broker)
        {
            SetDefaultValue(broker);
        }

        public override void ProcessPostValidate(BrokerModel broker, BrokerSchema schema)
        {
            ConvertToIntegerValue(broker);
            SetOptionNames(broker, schema);
        }

        private void SetDefaultValue(BrokerModel broker)
        {
            foreach (var defaultkey in DefaultValues.Keys)
            {
                var existingKey = broker.Settings.Keys.FirstOrDefault(key => key.Equals(defaultkey, StringComparison.InvariantCultureIgnoreCase));
                if (existingKey is null)
                {
                    broker.Settings[defaultkey] = DefaultValues[defaultkey];
                    continue;
                }

                var existingValue = broker.Settings[existingKey];
                if (Convert.ToString(existingValue) == string.Empty)
                {
                    broker.Settings[existingKey] = DefaultValues[defaultkey];
                    continue;
                }
            }
        }

        private void ConvertToIntegerValue(BrokerModel broker)
        {
            foreach (var key in IntegerDataKeys)
            {
                var value = (decimal)broker.Settings[key];
                broker.Settings[key] = (int)value;
            }
        }

        private void SetOptionNames(BrokerModel broker, BrokerSchema schema)
        {
            var detailKey = "tier";
            var additionalKey = "tierName";

            var optionKey = broker.Settings[detailKey] as string;
            var option = schema.Details.Where(detail => detail.Key == detailKey && detail.Options.Any())
                                       .SelectMany(detail => detail.Options)
                                       .FirstOrDefault(option => option.Id == optionKey);

            broker.Settings[additionalKey] = option?.Name ?? string.Empty;
        }
    }
}