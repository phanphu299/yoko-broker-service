using System.Threading.Tasks;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.SharedKernel.Extension;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AHI.Broker.Function.Trigger.RabbitMQ
{
    public class FileExportProcessing
    {
        private readonly ITenantContext _tenantContext;
        private readonly IFileExportService _fileExportService;
        private readonly IAuditLogService _auditLogService;

        public FileExportProcessing(ITenantContext tenantContext, IFileExportService fileExportService, IAuditLogService auditLogService)
        {
            _tenantContext = tenantContext;
            _fileExportService = fileExportService;
            _auditLogService = auditLogService;
        }

        [FunctionName("FileExportProcessing")]
        public async Task RunAsync(
        [RabbitMQTrigger("broker.function.file.exported.processing", ConnectionStringSetting = "RabbitMQ")] byte[] data,
        ILogger log, ExecutionContext context)
        {
            BaseModel<ExportFileMessage> request = data.Deserialize<BaseModel<ExportFileMessage>>();
            var eventMessage = request.Message;

            // setup Domain to use inside repository
            _tenantContext.RetrieveFromString(eventMessage.TenantId, eventMessage.SubscriptionId, eventMessage.ProjectId);

            var result = await _fileExportService.ExportFileAsync(eventMessage.RequestedBy, eventMessage.ActivityId, context, eventMessage.ObjectType, eventMessage.Ids, eventMessage.DateTimeFormat, eventMessage.DateTimeOffset);
            await LogActivityAsync(result, eventMessage.RequestedBy);
        }
        private Task LogActivityAsync(ImportExportBasePayload message, string requestedBy)
        {
            var activityMessage = message.CreateLog(requestedBy, _tenantContext, _auditLogService.AppLevel);
            return _auditLogService.SendLogAsync(activityMessage);
        }
    }
}
