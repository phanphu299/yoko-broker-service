using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using JsonConstant = AHI.Infrastructure.SharedKernel.Extension.Constant;
using AHI.Broker.Function.Constant;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace AHI.Broker.Function.Service
{
    public class NotificationService : INotificationService
    {
        protected readonly IHttpClientFactory _clientFactory;
        protected readonly ILoggerAdapter<NotificationService> _logger;
        private readonly ITenantContext _tenantContext;
        public NotificationService(IHttpClientFactory clientFactory, ILoggerAdapter<NotificationService> logger, ITenantContext tenantContext)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _tenantContext = tenantContext;
        }

        public virtual async Task SendNotifyAsync(string endpoint, NotificationMessage message)
        {
            message.Payload = JsonConvert.SerializeObject(message.Payload, JsonConstant.JsonSerializerSetting);
            var httpClient = _clientFactory.CreateClient(HttpClientNames.NOTIFICATION_HUB, _tenantContext);
            var response = await httpClient.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, mediaType: "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError(content);
            }
        }
    }
}