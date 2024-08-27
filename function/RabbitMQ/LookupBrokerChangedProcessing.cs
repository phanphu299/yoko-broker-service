using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AHI.Broker.Function.Service.Abstraction;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Function.Model;

namespace AHI.Broker.Function.Trigger.RabbitMQ
{
    public class LookupBrokerChangedProcessing
    {
        private readonly ILookupService _lookupService;
        private readonly ITenantContext _tenantContext;

        public LookupBrokerChangedProcessing(ILookupService lookupService, ITenantContext tenantContext)
        {
            _lookupService = lookupService;
            _tenantContext = tenantContext;
        }

        [FunctionName("LookupBrokerChangedProcessing")]
        public Task RunAsync(
        [RabbitMQTrigger("broker.function.lookup.changed.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger logger)
        {

            BaseModel<LookupInfo> request = data.Deserialize<BaseModel<LookupInfo>>();
            var eventMessage = request.Message;
            _tenantContext.SetTenantId(eventMessage.TenantId);
            _tenantContext.SetSubscriptionId(eventMessage.SubscriptionId);
            _tenantContext.SetProjectId(eventMessage.ProjectId);
            return _lookupService.UpsertAsync(eventMessage);

        }
    }
}
