using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Function.Contant;
using Function.Service.Abstraction;
using Function.Service.Model;
using Microsoft.Extensions.Configuration;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;

namespace Function.Service
{
    public class WaylayService : IWaylayService
    {
        private readonly IDeviceService _deviceService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILoggerAdapter<WaylayService> _logger;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        public WaylayService(IDeviceService deviceService, IHttpClientFactory httpClientFactory, IConfiguration configuration, IDomainEventDispatcher domainEventDispatcher, ILoggerAdapter<WaylayService> logger)
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
                var tasks = devices.Select(deviceId => FetchWaylayInformationAsync(tenantId, subscriptionId, projectId, integrationIdString, deviceId));
                await Task.WhenAll(tasks);
            }
        }
        private async Task FetchWaylayInformationAsync(string tenantId, string subscriptionId, string projectId, string integrationId, string deviceId)
        {
            var waylayClient = _httpClientFactory.CreateClient(HttpClientName.WAYLAY);
            var url = $"resources/{deviceId}/series?metadata="; // change to series endpoint rather than current
            var responseMessage = await waylayClient.GetAsync(url);
            if (responseMessage.IsSuccessStatusCode)
            {
                var payload = await responseMessage.Content.ReadAsByteArrayAsync();
                var response = payload.Deserialize<IEnumerable<WaylaySeriesResponse>>();
                var groups = response.GroupBy(x => x.Latest.Timestamp);
                foreach (var item in groups)
                {
                    var dictionaryMessage = new Dictionary<string, object>();
                    foreach (var metric in item)
                    {
                        dictionaryMessage[metric.Name] = metric.Latest.Value;
                    }
                    dictionaryMessage["timestamp"] = item.Key;
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
            else
            {
                var content = await responseMessage.Content.ReadAsStringAsync();
                _logger.LogError("Fetching error", content);
            }
        }
    }
}