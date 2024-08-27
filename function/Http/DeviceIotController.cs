using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Function.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.Exception;

namespace Function.Http
{
    public class DeviceIotController
    {
        private readonly ITenantContext _tenantContext;
        private readonly IDeviceIotService _deviceIotService;
        public DeviceIotController(ITenantContext tenantContext, IDeviceIotService deviceIotService)
        {
            _tenantContext = tenantContext;
            _deviceIotService = deviceIotService;
        }

        [FunctionName("PushDeviceConfiguration")]
        public async Task<IActionResult> RunAsync(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/device/push")] HttpRequestMessage req
        )
        {
            ClaimsPrincipal principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<DeviceIotRequestMessage>(payload);
            var response = await _deviceIotService.PushMessageToDeviceIotAsync(message);
            return new ResponseMessageResult(response);
        }

        [FunctionName("RegisterDevice")]
        public async Task<IActionResult> RegisterDeviceAsync(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/device/register")] HttpRequestMessage req
        )
        {
            ClaimsPrincipal principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<DeviceIotRequestMessage>(payload);
            try
            {
                _tenantContext.SetTenantId(message.TenantId);
                _tenantContext.SetSubscriptionId(message.SubscriptionId);
                _tenantContext.SetProjectId(message.ProjectId);
                var response = await _deviceIotService.RegisterDeviceIotAsync(message);
                return new OkObjectResult(new { IsSuccess = true, Payload = response });
            }
            catch (EntityValidationException entityExists)
            {
                return new BadRequestObjectResult(entityExists);
            }
        }

        [FunctionName("UnregisterDevice")]
        public async Task<IActionResult> UnregisterDeviceAsync(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/device/unregister")] HttpRequestMessage req
        )
        {
            ClaimsPrincipal principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<DeviceIotRequestMessage>(payload);
            var response = await _deviceIotService.UnRegisterAsync(message);
            return new OkObjectResult(new { IsSuccess = true, Payload = response });
        }

        [FunctionName("RegeneratePrimaryKey")]
        public async Task<IActionResult> RegeneratePrimaryKeyAsync(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/device/key/regenerate")] HttpRequestMessage req
        )
        {
            ClaimsPrincipal principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<DeviceIotRequestMessage>(payload);
            await _deviceIotService.RegeneratePrimaryKeyAsync(message);
            return new OkResult();
        }

        [FunctionName("CheckExistingDevice")]
        public async Task<IActionResult> CheckExistingDeviceAsync(
                [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/device/exist")] HttpRequestMessage req
        )
        {
            ClaimsPrincipal principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.Content.ReadAsStringAsync();
            var message = JsonConvert.DeserializeObject<DeviceIotRequestMessage>(payload);
            _tenantContext.SetTenantId(message.TenantId);
            _tenantContext.SetSubscriptionId(message.SubscriptionId);
            _tenantContext.SetProjectId(message.ProjectId);

            var brokerContent = _deviceIotService.ParseBrokerContent(message.BrokerContent);
            var response = await _deviceIotService.CheckExistingDeviceAsync(message.DeviceId, brokerContent.iotHubName, brokerContent.iotHubId);
            bool exist = response.StatusCode == System.Net.HttpStatusCode.OK;
            return new OkObjectResult(new { IsSuccess = true, Payload = exist });
        }
    }
}
