using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using System.Collections.Generic;
using AHI.Infrastructure.SharedKernel.Extension;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Function.Timer
{
    public class SensorSimulation
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        public SensorSimulation(IConfiguration configuration, IHttpClientFactory httpClientFactory, IDomainEventDispatcher domainEventDispatcher)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _domainEventDispatcher = domainEventDispatcher;
        }

        [FunctionName("SensorSimulation")]
        public async Task RunAsync([TimerTrigger("%ScheduleCron%")] TimerInfo myTimer,
        [EventHub("%EventHubName%", Connection = "EventHubConnection")] IAsyncCollector<string> outputs,
        ILogger log)
        {
            var jsonTemplate = _configuration["JsonTemplate"];
            var deviceId = _configuration["DeviceId"];
            var numberOfDevices = Convert.ToInt32(_configuration["NumberOfDevices"] ?? "2");
            IEnumerable<string> deviceIds = new List<string>();
            if (numberOfDevices > 1)
            {
                var deviceRanges = Enumerable.Range(1, numberOfDevices);
                deviceIds = deviceRanges.Select(num => $"{deviceId}-{num}");
            }
            else
            {
                deviceIds = new[] { deviceId };
            }
            var httpClient = _httpClientFactory.CreateClient("json");
            var originContent = await httpClient.GetStringAsync(jsonTemplate);
            var timestamp = ConvertToUnixTimestamp(DateTime.UtcNow);
            originContent = originContent.Replace("@timestamp", timestamp.ToString());

            var contents = deviceIds.Select(deviceId =>
            {
                var rand = new Random();
                var intRand1 = rand.Next(1, 100);
                var intRand2 = rand.Next(1, 100);
                var intRand3 = rand.Next(1, 100);
                var intRand4 = rand.Next(1, 100);
                var intRand5 = rand.Next(1, 100);
                var intRand6 = rand.Next(1, 100);
                var intRand7 = rand.Next(1, 100);
                var intRand8 = rand.Next(1, 100);
                var intRand9 = rand.Next(1, 100);
                var intRand10 = rand.Next(1, 100);
                var intRand11 = rand.Next(1, 100);
                var intRand12 = rand.Next(1, 100);
                var intRand13 = rand.Next(1, 100);
                var intRand14 = rand.Next(1, 100);
                var intRand15 = rand.Next(1, 100);
                var intRand16 = rand.Next(1, 100);
                var intRand17 = rand.Next(1, 100);
                var intRand18 = rand.Next(1, 100);
                var intRand19 = rand.Next(1, 100);
                var intRand20 = rand.Next(1, 100);

                var content = originContent;
                content = content.Replace("@timestamp", timestamp.ToString());
                content = content.Replace("@intRand1", intRand1.ToString());
                content = content.Replace("@intRand2", intRand2.ToString());
                content = content.Replace("@intRand3", intRand3.ToString());
                content = content.Replace("@intRand4", intRand4.ToString());
                content = content.Replace("@intRand5", intRand5.ToString());
                content = content.Replace("@intRand6", intRand6.ToString());
                content = content.Replace("@intRand7", intRand7.ToString());
                content = content.Replace("@intRand8", intRand8.ToString());
                content = content.Replace("@intRand9", intRand9.ToString());
                content = content.Replace("@intRand10", intRand10.ToString());
                content = content.Replace("@intRand11", intRand11.ToString());
                content = content.Replace("@intRand12", intRand12.ToString());
                content = content.Replace("@intRand13", intRand13.ToString());
                content = content.Replace("@intRand14", intRand14.ToString());
                content = content.Replace("@intRand15", intRand15.ToString());
                content = content.Replace("@intRand16", intRand16.ToString());
                content = content.Replace("@intRand17", intRand17.ToString());
                content = content.Replace("@intRand18", intRand18.ToString());
                content = content.Replace("@intRand19", intRand19.ToString());
                content = content.Replace("@intRand20", intRand20.ToString());

                var floatRand1 = rand.NextDouble() * 100;
                var floatRand2 = rand.NextDouble() * 100;
                var floatRand3 = rand.NextDouble() * 100;
                var floatRand4 = rand.NextDouble() * 100;
                var floatRand5 = rand.NextDouble() * 100;
                var floatRand6 = rand.NextDouble() * 100;
                var floatRand7 = rand.NextDouble() * 100;
                var floatRand8 = rand.NextDouble() * 100;
                var floatRand9 = rand.NextDouble() * 100;
                var floatRand10 = rand.NextDouble() * 100;
                var floatRand11 = rand.NextDouble() * 100;
                var floatRand12 = rand.NextDouble() * 100;
                var floatRand13 = rand.NextDouble() * 100;
                var floatRand14 = rand.NextDouble() * 100;
                var floatRand15 = rand.NextDouble() * 100;
                var floatRand16 = rand.NextDouble() * 100;
                var floatRand17 = rand.NextDouble() * 100;
                var floatRand18 = rand.NextDouble() * 100;
                var floatRand19 = rand.NextDouble() * 100;
                var floatRand20 = rand.NextDouble() * 100;
                content = content.Replace("@floatRand1", floatRand1.ToString());
                content = content.Replace("@floatRand2", floatRand2.ToString());
                content = content.Replace("@floatRand3", floatRand3.ToString());
                content = content.Replace("@floatRand4", floatRand4.ToString());
                content = content.Replace("@floatRand5", floatRand5.ToString());
                content = content.Replace("@floatRand6", floatRand6.ToString());
                content = content.Replace("@floatRand7", floatRand7.ToString());
                content = content.Replace("@floatRand8", floatRand8.ToString());
                content = content.Replace("@floatRand9", floatRand9.ToString());
                content = content.Replace("@floatRand10", floatRand10.ToString());
                content = content.Replace("@floatRand11", floatRand11.ToString());
                content = content.Replace("@floatRand12", floatRand12.ToString());
                content = content.Replace("@floatRand13", floatRand13.ToString());
                content = content.Replace("@floatRand14", floatRand14.ToString());
                content = content.Replace("@floatRand15", floatRand15.ToString());
                content = content.Replace("@floatRand16", floatRand16.ToString());
                content = content.Replace("@floatRand17", floatRand17.ToString());
                content = content.Replace("@floatRand18", floatRand18.ToString());
                content = content.Replace("@floatRand19", floatRand19.ToString());
                content = content.Replace("@floatRand20", floatRand20.ToString());
                content = content.Replace("@deviceId", deviceId);
                return content;
            });

            var targetSoucrce = _configuration["TargetSource"] ?? "EventHub";
            if (targetSoucrce == "RabbitMQ")
            {
                var tasks = contents.Select(content => SendToRabbitMQAsync(content));
                await Task.WhenAll(tasks);
            }
            else
            {
                var tasks = contents.Select(content => outputs.AddAsync(content));
                await Task.WhenAll(tasks);
            }
        }

        private async Task SendToRabbitMQAsync(string content)
        {
            var jObject = JsonConvert.DeserializeObject<JObject>(content);
            IDictionary<string, object> request = JsonExtension.ParseJObject("", jObject, parseJArray: false);
            var projectInfo = _configuration["ProjectInfo"].Split(';');
            var tasks = new List<Task>();
            foreach (var info in projectInfo)
            {
                var pInfo = info.Split('_');
                var tenantId = pInfo[0];
                var subscriptionId = pInfo[1];
                var projectId = pInfo[2];
                request["tenantId"] = tenantId;
                request["subscriptionId"] = subscriptionId;
                request["projectId"] = projectId;
                var message = new IngestionMessage(request);
                tasks.Add(_domainEventDispatcher.SendAsync(message));
            }
            await Task.WhenAll(tasks);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
    public class IngestionMessage : BusEvent
    {
        public override string TopicName => "ingestion-exchange";
        public IDictionary<string, object> RawData { get; set; }
        public IngestionMessage(IDictionary<string, object> payload)
        {
            RawData = payload;
        }
    }
}
