using Function.Service.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Function.Http
{
    public class HealthCheckController
    {
        private readonly IWaylayService _waylayService;
        public HealthCheckController(IWaylayService waylayService)
        {
            _waylayService = waylayService;
        }
        [FunctionName("HealthProbeCheck")]
        public async Task<IActionResult> LivenessProbeCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fnc/healthz")] HttpRequestMessage req, ILogger logger)
        {
            await _waylayService.FetchDataAsync();
            return new OkResult();
        }
    }

}