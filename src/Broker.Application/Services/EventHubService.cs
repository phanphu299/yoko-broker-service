using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Broker.Application.Constant;
using System.Net.Http;
using AHI.Infrastructure.SharedKernel.Extension;
using Newtonsoft.Json;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Broker.Application.Repository;

namespace Broker.Application.Service
{
    public class EventHubService : IEventHubService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter<EventHubService> _logger;

        public EventHubService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, ILoggerAdapter<EventHubService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<IEnumerable<EventHubDto>> GetAllEventHubAsync(GetValidEventHub request, CancellationToken cancellationToken)
        {
            var brokers = await _unitOfWork.Brokers.AsQueryable().Include(x => x.Detail).Where(x => x.Detail != null && x.Type == BrokerTypeConstants.EVENT_HUB).Select(x => x).ToListAsync();
            var iotHubs = await _unitOfWork.Brokers.AsQueryable().Include(x => x.Detail).Where(x => x.Detail != null && x.Type == BrokerTypeConstants.IOT_HUB).Select(x => x).ToListAsync();
            var integrations = await _unitOfWork.Integrations.AsQueryable().Include(x => x.Detail).Where(x => x.Detail != null && x.Type == BrokerTypeConstants.INTEGRATION_EVENT_HUB).Select(x => x).ToListAsync();
            var result = new List<EventHubDto>();
            foreach (var broker in brokers)
            {
                var content = JsonConvert.DeserializeObject<EventHubInformation>(broker.Detail.Content);
                var key = await GetListenConnectionStringAsync(content);
                if (!string.IsNullOrEmpty(key))
                {
                    result.Add(new EventHubDto()
                    {
                        ConnectionString = key,
                        HubName = content.EventHubName,
                        Id = broker.Id,
                        Type = broker.Type,
                        Status = broker.Status
                    });
                }
            }
            foreach (var iotHub in iotHubs)
            {
                var content = JsonConvert.DeserializeObject<IoTHubInformation>(iotHub.Detail.Content);
                //var key = await GetListenConnectionStringAsync(content);
                if (!string.IsNullOrEmpty(content.ConnectionString))
                {
                    result.Add(new EventHubDto()
                    {
                        ConnectionString = content.ConnectionString,
                        HubName = content.EventHubName,
                        Id = iotHub.Id,
                        Type = iotHub.Type,
                        Status = iotHub.Status
                    });
                }
            }
            foreach (var integration in integrations)
            {
                var content = JsonConvert.DeserializeObject<EventHubInformation>(integration.Detail.Content);
                var key = content.ConnectionString;
                if (!string.IsNullOrEmpty(key))
                {
                    result.Add(new EventHubDto()
                    {
                        ConnectionString = key,
                        HubName = content.EventHubName,
                        Id = integration.Id,
                        Type = integration.Type,
                        Status = integration.Status
                    });
                }
            }
            return result;
        }
        private async Task<string> GetListenConnectionStringAsync(EventHubInformation information)
        {
            try
            {
                var azureClient = _httpClientFactory.CreateClient("azure-service");
                var message = await azureClient.PostAsync($"{information.EventHubId}/authorizationRules/Listen/listKeys?api-version={AzureContants.AZURE_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
                if (message.IsSuccessStatusCode)
                {
                    var payload = await message.Content.ReadAsByteArrayAsync();
                    var key = payload.Deserialize<AzureKeyResponse>();
                    return key.PrimaryConnectionString;
                }
            }
            catch (System.Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
            return null;
        }
        // To be removed base on US: https://dev.azure.com/ThanhTrungBui/yokogawa-ppm/_workitems/edit/16272/
        // private async Task<string> GetListenConnectionStringAsync(IoTHubInformation information)
        // {
        //     // https://docs.microsoft.com/en-us/rest/api/iothub/iot-hub-resource/get-keys-for-key-name
        //     try
        //     {
        //         string keyName = "service";
        //         var azureClient = _httpClientFactory.CreateClient("azure-service");
        //         var message = await azureClient.PostAsync($"{information.IoTHubId}/IotHubKeys/{keyName}/listKeys?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
        //         message.EnsureSuccessStatusCode();
        //         var payload = await message.Content.ReadAsByteArrayAsync();
        //         var key = payload.Deserialize<SharedAccessSignatureAuthorizationRule>();
        //         var primaryKey = key.PrimaryKey;
        //         var eventHubRequest = await azureClient.GetAsync($"{information.IoTHubId}?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}");
        //         eventHubRequest.EnsureSuccessStatusCode();
        //         var eventHubBody = await eventHubRequest.Content.ReadAsByteArrayAsync();
        //         var eventHub = eventHubBody.Deserialize<IoTHubDto>();
        //         return $"Endpoint={eventHub.Properties.EventHubEndpoints.Events.Endpoint};SharedAccessKeyName={keyName};SharedAccessKey={primaryKey};EntityPath={eventHub.Properties.EventHubEndpoints.Events.Path}";
        //     }
        //     catch (System.Exception exc)
        //     {
        //         _logger.LogError(exc, exc.Message);
        //     }
        //     return null;
        // }
    }
}
