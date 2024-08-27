using System.Collections.Generic;
using Configuration.Application.Constant;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
namespace Broker.Application.Events
{
    public class FileImportEvent : BusEvent
    {
        public override string TopicName => EventTopics.IMPORT_TOPIC;
        public string ObjectType { get; set; }
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public IEnumerable<string> FileNames { get; set; }
        public string RequestedBy { get; set; }

        public FileImportEvent(string objectType, IEnumerable<string> fileNames, ITenantContext tenantContext, string requestedBy)
        {
            ObjectType = objectType;
            TenantId = tenantContext.TenantId;
            SubscriptionId = tenantContext.SubscriptionId;
            ProjectId = tenantContext.ProjectId;
            FileNames = fileNames;
            RequestedBy = requestedBy;
        }

    }
}
