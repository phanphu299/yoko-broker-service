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
    public class BrokerRestApiValidator : BaseBrokerPersistenceValidator
    {
        private const string ENDPOINT_INDEX = "endpoint";
        private const string APIKEY_INDEX = "api_key";

        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;

        public BrokerRestApiValidator(
            IConfiguration configuration, ITenantContext tenantContext,
            IDictionary<string, IImportTrackingService> errorHandlers)
        : base(errorHandlers)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
        }

        public override bool CanApply(string brokerType) => brokerType == BrokerType.BROKER_REST_API;

        public override void ProcessPreValidate(BrokerModel broker) { return; }

        public override void ProcessPostValidate(BrokerModel broker, BrokerSchema schema)
        {
            SetRestApiInfo(broker);
            SetActiveStatus(broker);
        }

        private void SetRestApiInfo(BrokerModel broker)
        {
            var id = GenerateApiId(broker.Id);
            broker.Settings[ENDPOINT_INDEX] = $"{_configuration["Api:PublicEndpoint"].Trim('/')}/fnc/bkr/api?id={id}";
            // broker.Settings[APIKEY_INDEX] = GenerateApiKey();  // May implement later
        }

        private string GenerateApiId(Guid brokerId)
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