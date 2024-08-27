using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Function.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Function.Http
{
    public class BrokersController
    {
        private readonly IBrokerService _brokerService;
        private readonly ITenantContext _tenantContext;
        private readonly ILoggerAdapter<BrokersController> _logger;
        public BrokersController(IBrokerService brokerService, ITenantContext tenantContext, ILoggerAdapter<BrokersController> logger)
        {
            _brokerService = brokerService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        [FunctionName("CheckMqttAcl")]
        public async Task<IActionResult> CheckMqttAclAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/emqx/acl/check")] HttpRequestMessage req)
        {
            var payload = await req.Content.ReadAsStringAsync();
            _logger.LogDebug($"PAYLOAD ------ {payload}");
            var message = JsonConvert.DeserializeObject<CheckMqttAclRequest>(payload);

            bool isValid = await _brokerService.CheckMqttAclAsync(message);
            _logger.LogDebug($"IS ALLOWED ------ {isValid}");
            return new OkObjectResult(new { result = isValid ? "allow" : "deny" });
        }

        [FunctionName("EmqxAuthentication")]
        public async Task<IActionResult> CheckEmqxAuthenticationAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/emqx/auth/check")] HttpRequestMessage req)
        {
            var payload = await req.Content.ReadAsStringAsync();
            _logger.LogDebug($"PAYLOAD ------ {payload}");
            var message = JsonConvert.DeserializeObject<CheckEmqxAuthenticationRequest>(payload);

            bool isValid = await _brokerService.CheckEmqxAuthenticationAsync(message);
            _logger.LogDebug($"IS AUTHENTICATED ------ {isValid}");
            return new OkObjectResult(new { result = isValid ? "allow" : "deny" });
        }

        [FunctionName("AssignClient")]
        public async Task<IActionResult> AssignDeviceAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/emqx/assign/client")] HttpRequestMessage req)
        {
            var principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            _tenantContext.RetrieveFromHeader(req.Headers);
            var payload = await req.Content.ReadAsStringAsync();
            _logger.LogDebug($"AssignClient payload - {payload}");
            var message = JsonConvert.DeserializeObject<AssignClientRequest>(payload);

            await _brokerService.AssignClientAsync(message);
            return new OkResult();
        }

        [FunctionName("RemoveClient")]
        public async Task<IActionResult> RemoveDeviceAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/emqx/remove/client")] HttpRequestMessage req)
        {
            var principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            _tenantContext.RetrieveFromHeader(req.Headers);
            var payload = await req.Content.ReadAsStringAsync();
            _logger.LogDebug($"RemoveClient payload - {payload}");
            var message = JsonConvert.DeserializeObject<RemoveDeviceRequest>(payload);

            await _brokerService.RemoveClientAsync(message);
            return new OkResult();
        }

        [FunctionName("RemoveEmqxBrokers")]
        public async Task<IActionResult> RemoveEmqxBrokersAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/emqx/remove/brokers")] HttpRequestMessage req)
        {
            var principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            _tenantContext.RetrieveFromHeader(req.Headers);
            var payload = await req.Content.ReadAsStringAsync();
            _logger.LogDebug($"RemoveEmqxBrokers payload - {payload}");
            var message = JsonConvert.DeserializeObject<RemoveEmqxBrokersRequest>(payload);

            await _brokerService.RemoveEmqxBrokersAsync(message);
            return new OkResult();
        }

        [FunctionName("GetBrokerTopics")]
        public async Task<IActionResult> GetBrokerTopicsAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/emqx/topics")] HttpRequestMessage req)
        {
            var principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            _tenantContext.RetrieveFromHeader(req.Headers);
            var payload = await req.Content.ReadAsStringAsync();
            _logger.LogDebug($"GetBrokerTopics payload - {payload}");
            var message = JsonConvert.DeserializeObject<GetBrokerTopicsRequest>(payload);

            var result = await _brokerService.GetBrokerTopicsAsync(message);
            return new OkObjectResult(result);
        }
    }
}
