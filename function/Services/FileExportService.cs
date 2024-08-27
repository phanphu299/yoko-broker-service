using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using AHI.Broker.Function.Service.Abstraction;
using Microsoft.Azure.WebJobs;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Broker.Function.FileParser.Constant;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Broker.Function.Extension;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Constant;
using AHI.Broker.Function.FileParser.ErrorTracking.Model;
using ValidationMessage = AHI.Broker.Function.Constant.MessageConstants.FluentValidation;

namespace AHI.Broker.Function.Service
{
    public class FileExportService : IFileExportService
    {
        private readonly IExportNotificationService _notification;
        private readonly IExportTrackingService _errorService;
        private readonly IParserContext _context;
        private readonly IDictionary<string, IExportHandler> _exportHandler;
        private readonly ILoggerAdapter<FileExportService> _logger;

        public FileExportService(IExportNotificationService notificationService,
                                 IExportTrackingService errorService,
                                 IParserContext parserContext,
                                 IDictionary<string, IExportHandler> exportHandler,
                                 ILoggerAdapter<FileExportService> logger)
        {
            _notification = notificationService;
            _errorService = errorService;
            _context = parserContext;
            _exportHandler = exportHandler;
            _logger = logger;
        }

        public async Task<ImportExportBasePayload> ExportFileAsync(string upn, Guid activityId, ExecutionContext context, string objectType, IEnumerable<string> ids,
                                                            string dateTimeFormat, string dateTimeOffset)
        {
            _context.SetContextFormat(ContextFormatKey.DATETIMEFORMAT, dateTimeFormat);
            _context.SetContextFormat(ContextFormatKey.DATETIMEOFFSET, dateTimeOffset);

            _notification.Upn = upn;
            _notification.ActivityId = activityId;
            _notification.ObjectType = objectType;
            _notification.NotificationType = ActionType.Export;

            await _notification.SendStartNotifyAsync(ids.Count());
            try
            {
                if (_exportHandler.TryGetValue(objectType, out var handler))
                {
                    var downloadUrl = await handler.HandleAsync(context, ids);
                    if (!string.IsNullOrEmpty(downloadUrl))
                    {
                        _notification.URL = downloadUrl;
                    }
                }
                else
                    _errorService.RegisterError(ValidationMessage.EXPORT_NOT_SUPPORTED);
            }
            catch (Exception e)
            {
                _errorService.RegisterError(e.Message);
                _logger.LogError(e, e.Message);
            }
            var status = GetFinishStatus();
            var payload = await _notification.SendFinishExportNotifyAsync(status);
            return CreateLogPayload(payload);
        }

        private ActionStatus GetFinishStatus()
        {
            return _errorService.HasError ? ActionStatus.Fail : ActionStatus.Success;
        }

        private ImportExportLogPayload<TrackError> CreateLogPayload(ImportExportNotifyPayload payload)
        {
            var detail = new[] { new ExportPayload<TrackError>((payload as ExportNotifyPayload).URL, _errorService.GetErrors) };
            return new ImportExportLogPayload<TrackError>(payload)
            {
                Detail = detail
            };
        }
    }
}
