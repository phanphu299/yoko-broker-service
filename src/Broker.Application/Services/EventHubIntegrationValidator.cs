using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Consumer;
using Broker.Application.Service.Abstraction;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Messaging.EventHubs;
using System.Threading;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Broker.Application.Constant;
using FluentValidation.Results;
using Configuration.Application.Constant;

namespace Broker.Application.Service
{
    public class EventHubIntegrationValidator : IntegrationValidator
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerAdapter<EventHubIntegrationValidator> _logger;
        public EventHubIntegrationValidator(ISchemaService schemaService, IConfiguration configuration, ILoggerAdapter<EventHubIntegrationValidator> logger) : base(schemaService)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override async Task<IEnumerable<ValidationFailure>> ValidateAsync(string type, IDictionary<string, object> payload)
        {
            var validationFailures = new List<ValidationFailure>();
            await base.ValidateAsync(type, payload);
            if (type.Contains("INTEGRATION_EVENT_HUB", System.StringComparison.InvariantCultureIgnoreCase))
            {
                // get the connectionstring
                string connectionString = null;
                if (payload.ContainsKey("connection_string"))
                {
                    connectionString = payload["connection_string"] as string;
                }
                if (payload.ContainsKey("connectionString"))
                {
                    connectionString = payload["connectionString"] as string;
                }
                if (!string.IsNullOrEmpty(connectionString))
                {
                    var eventHubName = payload["event_hub_name"] as string;
                    // validate event hub connection
                    validationFailures.AddRange(await ValidateEventHubAsync(eventHubName, connectionString));
                }
            }
            return validationFailures;
        }

        private async Task<IEnumerable<ValidationFailure>> ValidateEventHubAsync(string eventHubName, string connectionString)
        {
            var validationFailures = new List<ValidationFailure>();
            try
            {
                // code take from: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send#receive-events
                var blobStorageConnectionString = _configuration["AzureWebJobsStorage"];
                if (blobStorageConnectionString == null)
                {
                    throw new GenericCommonException(MessageConstants.COMMON_ERROR_MISSED_CONFIG);
                }
                string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
                var storageClient = new BlobContainerClient(blobStorageConnectionString, "brokerservicetest");
                await storageClient.CreateIfNotExistsAsync();
                // Create an event processor client to process events in the event hub
                var processor = new EventProcessorClient(storageClient, consumerGroup, connectionString, eventHubName);
                processor.ProcessEventAsync += (e) => Task.CompletedTask;
                processor.ProcessErrorAsync += (e) => Task.CompletedTask;
                var cancellationToken = new CancellationTokenSource(10 * 1000);
                await processor.StartProcessingAsync(cancellationToken.Token);
                await processor.StopProcessingAsync(cancellationToken.Token);
            }
            catch (Azure.RequestFailedException)
            {
                validationFailures.Add(new ValidationFailure(IntegrationTypeConstants.EVENT_HUB_CONNECTION_STRING, ExceptionErrorCode.DetailCode.ERROR_VALIDATION_UNEXPECTED_FORMAT));
            }
            catch (System.Exception exc)
            {
                _logger.LogTrace(exc, exc.Message);
                validationFailures.Add(new ValidationFailure(IntegrationTypeConstants.EVENT_HUB_CONNECTION_STRING, ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID));
            }
            return validationFailures;
        }
    }
}
