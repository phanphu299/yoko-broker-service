using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using Newtonsoft.Json;

namespace AHI.Broker.Function.Service
{
    public class MasterService : IMasterService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MasterService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IEnumerable<EventHubDto>> GetAllEventHubsBrokersAsync()
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.MASTER_FUNCTION);
            var response = await httpClient.GetAsync($"fnc/mst/listeners/eventhubs");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            IEnumerable<EventHubDto> result = JsonConvert.DeserializeObject<IEnumerable<EventHubDto>>(content);
            return result;
        }
    }
}
