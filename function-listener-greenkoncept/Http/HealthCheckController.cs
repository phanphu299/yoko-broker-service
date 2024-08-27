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
        private readonly IGreenKonceptService _greenKonceptService;
        public HealthCheckController(IGreenKonceptService greenKonceptService)
        {
            _greenKonceptService = greenKonceptService;
        }
        [FunctionName("HealthProbeCheck")]
        public async Task<IActionResult> LivenessProbeCheck(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fnc/healthz")] HttpRequestMessage req, ILogger logger)
        {
            await _greenKonceptService.FetchDataAsync();
            return new OkResult();
        }
    }

}
