using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Broker.Function.Service.Model;
using AHI.Broker.Function.Constant;
using System.Collections.Generic;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;

namespace AHI.Broker.Function.Service
{
    public class MasterService : IMasterService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICache _cache;

        public MasterService(IHttpClientFactory httpClientFactory, ICache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(bool migrated = true, bool deleted = false)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.MASTER_FUNCTION);
            var response = await httpClient.GetAsync($"fnc/mst/projects?migrated={migrated}&deleted={deleted}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsByteArrayAsync();
            return content.Deserialize<IEnumerable<ProjectDto>>();
        }

        public async Task<IEnumerable<TenantDto>> GetAllTenantsAsync(bool migrated = true, bool deleted = false)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.MASTER_FUNCTION);
            var response = await httpClient.GetAsync($"fnc/mst/tenants?migrated={migrated}&deleted={deleted}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsByteArrayAsync();
            return content.Deserialize<IEnumerable<TenantDto>>();
        }
    }
}
