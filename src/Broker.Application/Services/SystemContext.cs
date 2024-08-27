using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Broker.Application.Service.Abstractions;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.Cache.Abstraction;
using Broker.Application.Constants;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace Broker.Application.Service
{
    public class SystemContext : ISystemContext
    {
        private readonly ICache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITenantContext _tenantContext;
        private readonly ILoggerAdapter<SystemContext> _logger;

        public SystemContext(ICache cache, IHttpClientFactory httpClientFactory, ITenantContext tenantContext, ILoggerAdapter<SystemContext> logger)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _tenantContext = tenantContext;
            _logger = logger;
        }
        public async Task<string> GetValueAsync(string key, string defaultValue, bool useCache = true)
        {
            var cacheKey = $"{_tenantContext.ProjectId}_{key}";
            var cacheItem = await _cache.GetStringAsync(cacheKey);
            if (cacheItem == null || useCache == false)
            {
                // cache miss or force clear cache
                // find in system config
                var systemConfig = await GetFromServiceAsync(key, defaultValue);
                cacheItem = systemConfig.Value;
                await _cache.StoreAsync(cacheKey, cacheItem);
            }
            return cacheItem;

        }

        private async Task<SystemConfigDto> GetFromServiceAsync(string key, string defaultValue)
        {
            using var httpClient = _httpClientFactory.CreateClient(HttpClientNames.CONFIGURATION, _tenantContext);
            var response = await httpClient.GetAsync($"cnm/configs?key={key}");
            //response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadAsByteArrayAsync();
            try
            {
                var body = payload.Deserialize<BaseSearchResponse<SystemConfigDto>>();
                return body.Data.ElementAt(0);
            }
            catch (System.Exception exc)
            {
                _logger.LogError(exc, exc.Message);
                return new SystemConfigDto(key, defaultValue);
            }
        }

    }
    class SystemConfigDto
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public SystemConfigDto(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
