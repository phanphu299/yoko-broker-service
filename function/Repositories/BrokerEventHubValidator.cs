using System.Collections.Generic;
using System.Linq;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.Model.ImportModel;
using ValidationMessage = AHI.Broker.Function.Constant.MessageConstants.FluentValidation;

namespace AHI.Infrastructure.Repository
{
    public class BrokerEventHubValidator : BaseBrokerPersistenceValidator
    {
        private const string EVENT_HUB_NAME = "Hub Name";
        private const string EVENT_HUB_NAME_KEY = "event_hub_name";
        private const int EVENT_HUB_NAME_LIMIT = 255;
        private static readonly string[] IntegerDataKeys = new[] { "throughput_units", "max_throughput_units" };

        public BrokerEventHubValidator(IDictionary<string, IImportTrackingService> errorHandlers) : base(errorHandlers)
        {
        }

        public override bool CanApply(string brokerType) => brokerType == BrokerType.BROKER_EVENT_HUB;

        public override void ProcessPreValidate(BrokerModel broker) { return; }

        public override bool ValidateAdditionalCondition(BrokerModel broker)
        {
            return ValidateHubNameLimit(broker);
        }

        public override void ProcessPostValidate(BrokerModel broker, BrokerSchema schema)
        {
            ConvertToIntegerValue(broker);
            SetOptionNames(broker, schema);
        }

        private bool ValidateHubNameLimit(BrokerModel broker)
        {
            var eventHubName = broker.Settings[EVENT_HUB_NAME_KEY] as string;
            if (eventHubName.Length <= EVENT_HUB_NAME_LIMIT)
                return true;

            _errorService.RegisterError(ValidationMessage.MAX_LENGTH, validationInfo: new Dictionary<string, object>
            {
                { "propertyName", EVENT_HUB_NAME },
                { "maxLength", EVENT_HUB_NAME_LIMIT }
            });
            return false;
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