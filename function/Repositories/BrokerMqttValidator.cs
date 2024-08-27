using System;
using System.Collections.Generic;
using HashidsNet;
using Microsoft.Extensions.Configuration;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Extension;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.Model.ImportModel;
using AHI.Infrastructure.MultiTenancy.Abstraction;

namespace AHI.Infrastructure.Repository
{
    public class BrokerMqttValidator : BaseBrokerPersistenceValidator
    {
        private const string TOPIC = "topic";
        private readonly ITenantContext _tenantContext;

        public BrokerMqttValidator(ITenantContext tenantContext, IDictionary<string, IImportTrackingService> errorHandlers) : base(errorHandlers)
        {
            _tenantContext = tenantContext;
        }

        public override bool CanApply(string brokerType) => brokerType == BrokerType.EMQX_MQTT;

        public override void ProcessPreValidate(BrokerModel broker) { return; }

        public override void ProcessPostValidate(BrokerModel broker, BrokerSchema schema)
        {
            SetMqttInfo(broker);
            SetActiveStatus(broker);
        }

        private void SetMqttInfo(BrokerModel broker)
        {
            var id = GenerateTopicId(broker.Id);
            broker.Settings[TOPIC] = $"brk-{id}";
            // broker.Settings[APIKEY_INDEX] = GenerateApiKey();  // May implement later
        }

        private string GenerateTopicId(Guid brokerId)
        {
            var hasher = new Hashids();
            var key = hasher.EncodeGuid(_tenantContext.TenantId, _tenantContext.SubscriptionId, _tenantContext.ProjectId, brokerId.ToString());
            return key;
        }

        private void SetActiveStatus(BrokerModel broker)
        {
            broker.Status = "AC";
        }
    }
}
