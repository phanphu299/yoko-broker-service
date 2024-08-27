using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.Audit.Model;

namespace AHI.Broker.Function.Trigger.RabbitMQ
{
    public class FileImportProcessing
    {
        private readonly ITenantContext _tenantContext;
        private readonly IFileImportService _importService;
        private readonly IAuditLogService _auditLogService;
        public FileImportProcessing(ITenantContext tenantContext, IFileImportService importService, IAuditLogService auditLogService)
        {
            _tenantContext = tenantContext;
            _importService = importService;
            _auditLogService = auditLogService;
        }

        [FunctionName("FileImportProcessing")]
        public async Task RunAsync(
        [RabbitMQTrigger("broker.function.file.imported.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger log, ExecutionContext context)
        {
            var activityId = Guid.NewGuid();
            BaseModel<ImportFileMessage> request = data.Deserialize<BaseModel<ImportFileMessage>>();
            var eventMessage = request.Message;
            // setup Domain to use inside repository
            _tenantContext.SetTenantId(eventMessage.TenantId);
            _tenantContext.SetSubscriptionId(eventMessage.SubscriptionId);
            _tenantContext.SetProjectId(eventMessage.ProjectId);

            var result = await _importService.ImportFileAsync(eventMessage.RequestedBy, activityId, context, eventMessage.ObjectType, eventMessage.FileNames);
            await LogActivityAsync(result, eventMessage.RequestedBy);
        }
        private Task LogActivityAsync(ImportExportBasePayload message, string requestedBy)
        {
            var activityMessage = message.CreateLog(requestedBy, _tenantContext, _auditLogService.AppLevel);
            return _auditLogService.SendLogAsync(activityMessage);
        }
    }
}
