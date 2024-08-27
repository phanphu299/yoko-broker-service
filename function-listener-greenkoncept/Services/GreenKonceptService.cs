using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Function.Contant;
using Function.Service.Abstraction;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using System.Linq;
using Function.Service.Model;

namespace Function.Service
{
    public class GreenKonceptService : IGreenKonceptService
    {
        private readonly IDeviceService _deviceService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILoggerAdapter<GreenKonceptService> _logger;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        public GreenKonceptService(IDeviceService deviceService, IHttpClientFactory httpClientFactory, IConfiguration configuration, IDomainEventDispatcher domainEventDispatcher, ILoggerAdapter<GreenKonceptService> logger)
        {
            _deviceService = deviceService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _domainEventDispatcher = domainEventDispatcher;
            _logger = logger;
        }
        public async Task FetchDataAsync()
        {
            var integrationIdString = _configuration["IntegrationId"];
            var tenantId = _configuration["TenantId"];
            var subscriptionId = _configuration["SubscriptionId"];
            var projectId = _configuration["ProjectId"];
            var integrationId = Guid.Parse(integrationIdString);
            var devices = await _deviceService.GetDeviceAsync(integrationId);
            if (devices != null && devices.Any())
            {
                //fetching the information
                var tasks = devices.Select(deviceId => FetchGreenKonceptInformationAsync(tenantId, subscriptionId, projectId, integrationIdString, deviceId));
                await Task.WhenAll(tasks);
            }
        }
        private async Task FetchGreenKonceptInformationAsync(string tenantId, string subscriptionId, string projectId, string integrationId, string deviceId)
        {
            //get token
            var token = await GetGreenKonceptTokenAsync();
            if (token == null)
            {
                _logger.LogError("Get token error: ", token);
                return;
            }

            //send to MQ
            await ReceiveMessagesGreenKonceptAsync(token, tenantId, subscriptionId, projectId, integrationId, deviceId);
        }

        private async Task<string> GetGreenKonceptTokenAsync()
        {
            var client = _httpClientFactory.CreateClient(HttpClientName.GREEN_KONCEPT);
            client.DefaultRequestHeaders.Clear();
            var tokenUrl = $"api/v1/auth/token";
            var content = new Dictionary<string, string>
            {
                ["clientId"] = _configuration["GreenKoncept:ClientId"],
                ["clientSecret"] = _configuration["GreenKoncept:ClientSecret"]
            };
            var responseMessage = await client.PostAsync(tokenUrl, new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));

            if (!responseMessage.IsSuccessStatusCode)
                return null;

            var message = await responseMessage.Content.ReadAsByteArrayAsync();
            return message.Deserialize<GreenKonceptTokenRespone>()?.AccessToken;
        }

        private async Task ReceiveMessagesGreenKonceptAsync(string accessToken, string tenantId, string subscriptionId, string projectId, string integrationId, string deviceId)
        {
            var greenClient = _httpClientFactory.CreateClient(HttpClientName.GREEN_KONCEPT);
            greenClient.DefaultRequestHeaders.Clear();
            greenClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            //get snapshot in 1 min before
            var tsStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var tsEnd = DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds();
            var measures = await FetchMeasuresByNodeUidAsync(deviceId, accessToken);
            if (!measures.Any())
                return;

            var timegrain = _configuration["GreenKoncept:Timegrain"]; //defaul 0 = min
            var url = $"/api/v1/iot/data-events?start-date={tsStart}&end-date={tsEnd}&bin={timegrain}";
            var payload = new List<GreenKonceptSeriesPayload>
            {
                new GreenKonceptSeriesPayload
                {
                    NodeUID = deviceId,
                    Measures = measures
                }
            };
            var responseMessage = await greenClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));
            if (!responseMessage.IsSuccessStatusCode)
            {
                var content = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogError("Fetching green error", content);
            }

            var responsePayload = await responseMessage.Content.ReadAsByteArrayAsync();
            var response = responsePayload.Deserialize<IEnumerable<GreenKonceptSeries>>();
            var groups = response.Select(x => x.Events.OrderByDescending(o => o.Timestamp).FirstOrDefault());
            foreach (var item in groups)
            {
                var dictionaryMessage = new Dictionary<string, object>();
                foreach (var metric in item.EventData)
                {
                    dictionaryMessage[metric.measure] = metric.measure;
                    dictionaryMessage["timestamp"] = item.Timestamp;
                    dictionaryMessage["tenantId"] = tenantId;
                    dictionaryMessage["subscriptionId"] = subscriptionId;
                    dictionaryMessage["projectId"] = projectId;
                    dictionaryMessage["deviceId"] = deviceId;
                    dictionaryMessage["integrationId"] = integrationId;
                    _logger.LogInformation($"Dispatch {integrationId}/{deviceId}");
                    var message = new IngestionMessage(dictionaryMessage);
                    await _domainEventDispatcher.SendAsync(message);
                }
            }
        }

        private async Task<IEnumerable<string>> FetchMeasuresByNodeUidAsync(string nodeUid, string accessToken)
        {
            var greenClient = _httpClientFactory.CreateClient(HttpClientName.GREEN_KONCEPT);
            greenClient.DefaultRequestHeaders.Clear();

            //fake accessToken
            //accessToken = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJZb2tvZ2F3YUVFTVMxIiwidG9rZW4tc2NvcGUiOiJBQ0NFU1NfVE9LRU4iLCJ1c2VyLWFjY291bnQtaWQiOjE5Miwic2NvcGVzIjoibnVsbCx2aWV3OmlvdCx2aWV3OnNldHRpbmdzLHZpZXc6cmVwb3J0cyx2aWV3OmVuZXJneSx2aWV3OmlhcSx2aWV3OmZhdWx0ZGIiLCJleHAiOjE2NTY4NjQ3ODgsInVzZXItYWNjb3VudC1uYW1lIjoieW9rb2dhd2Ffc2ciLCJ1c2VyLXN1Yi1hY2NvdW50LWlkIjpudWxsLCJpYXQiOjE2NTU5NzU3NTV9.QUqHQIwq3ZVCLSf-YVOQjOQB9XuWY6y4DaTso6LVjYdArUrNbu2Z2Ul_42szVYiJnBzC0XCnwW2T71ZSfyxI0g";

            greenClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            var response = await greenClient.GetByteArrayAsync($"api/v1/iot/node/{nodeUid}/measures");
            var responsePayload = response.Deserialize<IEnumerable<GreenKonceptMeasureData>>();
            return responsePayload.Select(x => x.DataName);
        }

    }
}
