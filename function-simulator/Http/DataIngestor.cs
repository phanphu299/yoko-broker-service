using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Abstraction;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using AHI.Broker.Function.Constant;
using System.Collections.Generic;
using System.Text;
using System.IO;
using AHI.Broker.Function.Models;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Broker.Function.Extension;
using Newtonsoft.Json;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.SharedKernel.Model;

namespace AHI.Broker.Function.Trigger.Http
{
    public class DataIngestor
    {

        private readonly ILoggerAdapter<DataIngestor> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ICache _cache;


        public DataIngestor(ILoggerAdapter<DataIngestor> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration, ICache cache)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _cache = cache;
        }

        [FunctionName("DataIngestor")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "delete", Route = "fnc/bkr/simulator")] HttpRequest req, ILogger logger
        )
        {
            bool isAuthenticated = false;
            var query = req.Query;
            var code = query["code"];
            if (string.Equals(code, _configuration["AuthorizationCode"], System.StringComparison.InvariantCultureIgnoreCase))
            {
                isAuthenticated = true;
            }
            if (!isAuthenticated)
            {
                return new UnauthorizedResult();
            }

            var contentStream = GetContent(req, out var contentType, out var fileName);
            var texts = new List<string>();
            using (var reader = new StreamReader(contentStream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    texts.Add(line);
                }
            }
            var jobName = $"data_simulator_{fileName}";
            var firstLine = texts.First();
            var lineInfo = firstLine.Split(",");
            texts.Remove(firstLine);
            var job = new JobInfo();
            job.Lines = texts;
            job.DeviceId = lineInfo[2].Split('=', 2)[1].Trim();
            job.Interval = int.Parse(lineInfo[3].Split('=', 2)[1].Trim());

            if (string.Equals("delete", req.Method, System.StringComparison.InvariantCultureIgnoreCase))
            {
                // delete the current job
                _logger.LogInformation($"Delete the current job: {jobName}");
                await _cache.DeleteAsync(jobName);
                var jobKey = $"data_simulator_job_list_{job.Interval}";
                var jobList = await _cache.GetAsync<List<string>>(jobKey);
                if (jobList == null)
                {
                    jobList = new List<string>();
                }
                jobList.RemoveAll(x => x == jobName);
                await _cache.StoreAsync(jobKey, jobList);
            }
            else
            {
                var projectInfo = await GetProjectInfoAsync(lineInfo[0].Split('=', 2)[1].Trim());
                var brokerInfo = await GetBrokerInfoAsync(lineInfo[1].Split('=', 2)[1].Trim(), projectInfo);
                job.Project = projectInfo;
                job.Broker = brokerInfo;
                await _cache.StoreAsync(jobName, job);
                var jobKey = $"data_simulator_job_list_{job.Interval}";
                var jobList = await _cache.GetAsync<List<string>>(jobKey);
                if (jobList == null)
                {
                    jobList = new List<string>();
                }
                jobList.RemoveAll(x => x == jobName);
                jobList.Add(jobName);
                _logger.LogInformation($"Add to job list: {jobName}");
                await _cache.StoreAsync(jobKey, jobList);
            }
            return new OkResult();
        }

        private async Task<BrokerInfo> GetBrokerInfoAsync(string brokerName, ProjectInfo projectInfo)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.BROKER_SERVICE);
            client.AddTenantContextHeader(projectInfo);
            var search = new
            {
                filter = JsonConvert.SerializeObject(new
                {
                    QueryKey = "name",
                    QueryType = "text",
                    QueryValue = brokerName,
                    Operation = "contains"
                })
            };
            var response = await client.PostAsync("bkr/brokers/search", new StringContent(JsonConvert.SerializeObject(search), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var brokerResponse = await response.Content.ReadAsByteArrayAsync();
            var brokers = brokerResponse.Deserialize<BaseSearchResponse<BrokerInfo>>();
            var broker = brokers.Data.First(x => x.Name == brokerName);
            var brokerDetail = await client.GetByteArrayAsync($"bkr/brokers/{broker.Id}");
            return brokerDetail.Deserialize<BrokerInfo>();

        }

        private async Task<ProjectInfo> GetProjectInfoAsync(string projectName)
        {
            var client = _httpClientFactory.CreateClient(HttpClientNames.MASTER_FUNCTION);
            var projectResponse = await client.GetByteArrayAsync("fnc/mst/projects?type=asset&migrated=true");
            var projects = projectResponse.Deserialize<IEnumerable<ProjectInfo>>();
            return projects.Where(x => x.Name == projectName).First();
        }

        private System.IO.Stream GetContent(HttpRequest request, out string contentType, out string fileName)
        {
            contentType = null;
            fileName = string.Empty;
            if (request.ContentType == MimeType.JSON)
            {
                contentType = MimeType.JSON;
                return request.Body;
            }
            else if (request.HasFormContentType)
            {
                var file = request.Form?.Files?.FirstOrDefault(x => x.ContentType == MimeType.CSV);
                if (file != null)
                {
                    fileName = file.FileName;
                    contentType = MimeType.CSV;
                    return file.OpenReadStream();
                }
            }
            return null;
        }
    }
}
