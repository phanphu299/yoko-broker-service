using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Broker.Function.FileParser.Constant;
using AHI.Broker.Function.FileParser.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Broker.Function.Extension;
using AHI.Infrastructure.Audit.Model;
using AHI.Infrastructure.Audit.Constant;
using AHI.Broker.Function.FileParser.ErrorTracking.Model;
using System.IO;

namespace AHI.Broker.Function.Service
{
    public class FileImportService : IFileImportService
    {
        private readonly IImportNotificationService _notification;
        private readonly IParserContext _context;
        private readonly IDictionary<Type, Infrastructure.Import.Abstraction.IFileImport> _importHandlers;
        private readonly IDictionary<string, IImportTrackingService> _errorHandlers;
        private readonly IStorageService _storageService;
        private readonly ILoggerAdapter<FileImportService> _logger;
        private IImportTrackingService _errorService;
        public FileImportService(IImportNotificationService notification,
                                 IDictionary<string, IImportTrackingService> errorHandlers,
                                 IDictionary<Type, Infrastructure.Import.Abstraction.IFileImport> importHandlers,
                                 IParserContext context,
                                 IStorageService storageService,
                                 ILoggerAdapter<FileImportService> logger)
        {
            _notification = notification;
            _errorHandlers = errorHandlers;
            _context = context;
            _importHandlers = importHandlers;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<ImportExportBasePayload> ImportFileAsync(string upn, Guid activityId, ExecutionContext context, string objectType, IEnumerable<string> fileNames)
        {
            var mimeType = Constant.EntityFileMapping.GetMimeType(objectType);
            var modelType = Constant.EntityFileMapping.GetEntityType(objectType);
            _errorService = _errorHandlers[mimeType];

            _context.SetExecutionContext(context, ParseAction.IMPORT);

            _notification.Upn = upn;
            _notification.ActivityId = activityId;
            _notification.ObjectType = objectType;
            _notification.NotificationType = ActionType.Import;

            // remove token and duplicate files
            var files = PreProcessFileNames(fileNames);

            // send signalR starting import
            await _notification.SendStartNotifyAsync(files.Count());

            foreach (string file in files)
            {
                _errorService.File = file;
                using (var stream = new System.IO.MemoryStream())
                {
                    await DownloadImportFileAsync(file, stream);
                    if (stream.CanRead)
                    {
                        try
                        {
                            var fileImport = _importHandlers[modelType];
                            await fileImport.ImportAsync(stream);
                        }
                        catch (Exception ex)
                        {
                            _errorService.RegisterError(ex.Message, Constant.ErrorType.UNDEFINED);
                            _logger.LogError(ex, ex.Message);
                        }
                    }
                }
            }
            var status = GetFinishStatus(out var partialInfo);
            var payload = await _notification.SendFinishImportNotifyAsync(status, partialInfo);
            return CreateLogPayload(payload);
        }

        private async Task DownloadImportFileAsync(string filePath, Stream outputStream)
        {
            try
            {
                await _storageService.DownloadFileToStreamAsync(filePath, outputStream);
            }
            catch
            {
                outputStream.Dispose();
                _errorService.RegisterError(ImportErrorMessage.IMPORT_ERROR_GET_FILE_FAILED, Constant.ErrorType.UNDEFINED);
            }
        }

        private IEnumerable<string> PreProcessFileNames(IEnumerable<string> fileNames)
        {
            return fileNames.Select(StringExtension.RemoveFileToken).Distinct();
        }

        private ActionStatus GetFinishStatus(out (int, int) partialInfo)
        {
            var total = _errorService.FileErrors.Keys.Count;
            var failCount = _errorService.FileErrors.Where(x => x.Value.Count > 0).Count();
            if (failCount == 0)
            {
                partialInfo = (total, 0);
                return ActionStatus.Success;
            }

            if (failCount == total)
            {
                partialInfo = (0, total);
                return ActionStatus.Fail;
            }

            var successCount = total - failCount;
            partialInfo = (successCount, failCount);
            return ActionStatus.Partial;
        }

        private ImportExportBasePayload CreateLogPayload(ImportExportNotifyPayload payload)
        {
            return new ImportExportLogPayload<TrackError>(payload)
            {
                Detail = _errorService.FileErrors.Select(x => new ImportPayload<TrackError>(x.Key, x.Value)).ToArray()
            };
        }
    }
}
