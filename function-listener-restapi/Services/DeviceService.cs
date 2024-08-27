using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace AHI.Broker.Function.Service
{
    public class DeviceService : IDeviceService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITenantContext _tenantContext;

        public DeviceService(IHttpClientFactory httpClientFactory, ITenantContext tenantContext)
        {
            _httpClientFactory = httpClientFactory;
            _tenantContext = tenantContext;
        }

        public async Task<BaseResponse> ValidateIngestionAsync(string filePath)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.DEVICE_FUNCTION, _tenantContext);

            var responseMessage = await client.PostAsync($"fnc/dev/api/ingestion/validate", new StringContent(JsonConvert.SerializeObject(new IngestionValidationDto()
            {
                FilePath = filePath,
                TenantId = _tenantContext.TenantId,
                SubscriptionId = _tenantContext.SubscriptionId,
                ProjectId = _tenantContext.ProjectId
            }), Encoding.UTF8, mediaType: "application/json"));
            responseMessage.EnsureSuccessStatusCode();

            var messages = await responseMessage.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<BaseResponse>(messages);
            return response;
        }
    }

    internal class IngestionValidationDto
    {
        public string TenantId { get; set; }
        public string ProjectId { get; set; }
        public string SubscriptionId { get; set; }
        public string FilePath { get; set; }
    }
}
