using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Exception;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Model.Event;
using AHI.Broker.Function.Parser;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;

namespace AHI.Broker.Function.Service
{
    public class DataIngestionService : IDataIngestionService
    {
        private readonly ITenantContext _tenantContext;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly IStorageService _storageService;
        private readonly IDeviceService _deviceService;
        private readonly IConfiguration _configuration;
        private readonly IDictionary<string, Func<Guid, byte[], string, Task>> _handlers;
        private readonly int DEFAULT_MAX_SIZE = 200; //Megabytes

        public DataIngestionService(
            ITenantContext tenantContext,
            IDomainEventDispatcher domainEventDispatcher,
            IStorageService storageService,
            IDeviceService deviceService,
            IConfiguration configuration
        )
        {
            _tenantContext = tenantContext;
            _domainEventDispatcher = domainEventDispatcher;
            _storageService = storageService;
            _deviceService = deviceService;
            _configuration = configuration;
            _handlers = new Dictionary<string, Func<Guid, byte[], string, Task>>
            {
                { MimeType.JSON, IngestJsonAsync },
                { MimeType.CSV, IngestCsvAsync }
            };
        }

        public async Task IngestDataAsync(Guid brokerId, string contentType, byte[] contentStream, string fileName)
        {
            if (_handlers.TryGetValue(contentType, out var handler))
            {
                await handler.Invoke(brokerId, contentStream, fileName);
                return;
            }
            throw new NotSupportedException($"Mimetype {contentType} not supported");
        }

        private async Task IngestJsonAsync(Guid brokerId, byte[] contentStream, string fileName)
        {
            var content = contentStream.Deserialize<JObject>();
            var request = JsonExtension.ParseJObject("", content);
            request["tenantId"] = _tenantContext.TenantId;
            request["subscriptionId"] = _tenantContext.SubscriptionId;
            request["projectId"] = _tenantContext.ProjectId;
            request["brokerId"] = brokerId.ToString();
            var message = new IngestionMessage(request, _tenantContext);
            await _domainEventDispatcher.SendAsync(message);
        }

        private async Task IngestCsvAsync(Guid brokerId, byte[] data, string fileName)
        {
            // Validate file size
            var maxFileSizeConfig = _configuration["MaxFileSize"];
            Int32.TryParse(maxFileSizeConfig, out int maxFileSize);
            if (maxFileSize <= 0)
                maxFileSize = DEFAULT_MAX_SIZE;

            if (data != null && data.Length > (maxFileSize * 1024 * 1024))
            {
                throw new FileIngestionException(DescriptionMessage.MAX_FILE_SIZE_EXCEEDED);
            }

            string filePath = string.Empty;
            filePath = await _storageService.UploadAsync($"sta/files/broker/{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}", fileName, data);

            // Failed upload
            if (string.IsNullOrEmpty(filePath))
            {
                throw new FileIngestionException(DescriptionMessage.UPLOAD_FAIL);
            }

            // Validate file content
            var validationResponse = await _deviceService.ValidateIngestionAsync(filePath);
            if (validationResponse != null && validationResponse.IsSuccess)
            {
                // Send an event to trigger a bulk import function
                var dataIngestionEvent = new DataIngestionEvent(filePath, _tenantContext);
                await _domainEventDispatcher.SendAsync(dataIngestionEvent);
            }
            else
            {
                throw new FileIngestionException(DescriptionMessage.INVALID_FILE_FORMAT);
            }
        }

        public Task IngestBatchDataAsync(Guid brokerId, byte[] rawInput)
        {
            using (var contentStream = new MemoryStream(rawInput))
            {
                var data = ParseData(brokerId, contentStream);
                return Task.WhenAll(data.Select(payload => _domainEventDispatcher.SendAsync(new IngestionMessage(payload, _tenantContext))));
            }
        }

        private IEnumerable<IDictionary<string, object>> ParseData(Guid brokerId, Stream contentStream)
        {
            foreach (var data in CsvParser.ParseCsvData(contentStream))
            {
                data["tenantId"] = _tenantContext.TenantId.ToString();
                data["subscriptionId"] = _tenantContext.SubscriptionId.ToString();
                data["projectId"] = _tenantContext.ProjectId.ToString();
                data["brokerId"] = brokerId.ToString();
                yield return data;
            }
        }
    }
}
