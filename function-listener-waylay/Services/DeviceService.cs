using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Function.Contant;
using Function.Service.Abstraction;
using Microsoft.Extensions.Configuration;
using Function.Extension;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;

namespace Function.Service
{
    public class DeviceService : IDeviceService
    {
        private readonly ICache _cache;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public DeviceService(ICache cache, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _cache = cache;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<IEnumerable<string>> GetDeviceAsync(Guid integrationId)
        {
            var tenantId = _configuration["TenantId"];
            var subscriptionId = _configuration["SubscriptionId"];
            var projectId = _configuration["ProjectId"];
            var key = $"{tenantId}_{subscriptionId}_{projectId}_integrationDevices_{integrationId}";
            var result = await _cache.GetAsync<IEnumerable<string>>(key);
            if (result == null || !result.Any())
            {
                // get from device-service
                var client = _httpClientFactory.CreateClient(HttpClientName.DEVICE);
                client.SetHeaders(_configuration);
                var responsePayload = await client.GetByteArrayAsync($"/dev/integrations/{integrationId}/devices");
                result = responsePayload.Deserialize<IEnumerable<string>>();
            }
            return result;
        }
    }
}