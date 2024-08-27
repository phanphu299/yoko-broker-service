using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AHI.Infrastructure.Cache.Abstraction;
using System.Collections.Generic;
using System.Linq;
using AHI.Broker.Function.Service.Abstraction;

namespace Function.Timer
{
    public class Data20MinSimulation
    {
        private readonly ICache _cache;
        private readonly IJobProcessing _jobProcessing;
        public Data20MinSimulation(ICache cache, IJobProcessing jobProcessing)
        {
            _cache = cache;
            _jobProcessing = jobProcessing;
        }

        [FunctionName("Data20MinSimulation")]
        public async Task RunAsync([TimerTrigger("0 */20 * * * *")] TimerInfo myTimer,
        ILogger log)
        {
            var jobKey = "data_simulator_job_list_1200";
            var jobs = await _cache.GetAsync<List<string>>(jobKey);
            if (jobs != null)
            {
                foreach (var job in jobs.ToList())
                {
                    await _jobProcessing.ProcessAsync(job);
                }
            }

        }
    }
}
