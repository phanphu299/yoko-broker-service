using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHI.Broker.Function.Event;
using AHI.Broker.Function.Extension;
using AHI.Broker.Function.Models;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Newtonsoft.Json;

namespace AHI.Broker.Function.Service
{
    public class JobProcessing : IJobProcessing
    {
        private readonly ICache _cache;
        private readonly ILoggerAdapter<JobProcessing> _logger;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private static string[] EVENT_HUB_TYPES = new[] { "BROKER_IOT_HUB", "BROKER_EVENT_HUB" };
        public JobProcessing(ICache cache, ILoggerAdapter<JobProcessing> logger, IDomainEventDispatcher domainEventDispatcher)
        {
            _cache = cache;
            _logger = logger;
            _domainEventDispatcher = domainEventDispatcher;
        }
        public async Task ProcessAsync(string jobName)
        {
            var jobDetail = await _cache.GetAsync<JobInfo>(jobName);
            if (jobDetail != null)
            {

                var header = jobDetail.Lines.ElementAt(0);
                if (jobDetail.CurrentLine >= jobDetail.Lines.Count - 1)
                {
                    // restart the line of the file
                    // system will start it again from line 2
                    jobDetail.CurrentLine = 1;
                }
                else
                {
                    // increase the current line of the file
                    jobDetail.CurrentLine++;
                }
                var line = jobDetail.Lines.ElementAt(jobDetail.CurrentLine);
                var payload = GetPayload(jobDetail.DeviceId, header, line);
                if (EVENT_HUB_TYPES.Contains(jobDetail.Broker.Type))
                {
                    await SendToEventHubAsync(jobDetail, payload);
                }
                else
                {
                    await SendToRabbitMqAsync(jobDetail, payload);
                }
                _logger.LogInformation($"Current line: {jobDetail.CurrentLine}");
                await _cache.StoreAsync(jobName, jobDetail);
            }
        }
        private async Task SendToEventHubAsync(JobInfo jobInfo, string payload)
        {
            var broker = JsonConvert.DeserializeObject<BrokerContent>(jobInfo.Broker.Content);
            EventHubProducerClient client = new EventHubProducerClient(broker.ConnectionString, broker.EventHubName);
            EventDataBatch eventBatch = await client.CreateBatchAsync();
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(payload)));
            _logger.LogInformation($"Send to event hub: {payload}");
            await client.SendAsync(eventBatch);
        }
        private async Task SendToRabbitMqAsync(JobInfo jobInfo, string payload)
        {
            _logger.LogInformation($"Send to rabbit mq: {payload}");
            var request = JsonConvert.DeserializeObject<IDictionary<string, object>>(payload);
            request["tenantId"] = jobInfo.Project.TenantId;
            request["subscriptionId"] = jobInfo.Project.SubscriptionId;
            request["projectId"] = jobInfo.Project.Id;
            var message = new IngestionMessage(request);
            await _domainEventDispatcher.SendAsync(message);
        }
        private string GetPayload(string deviceId, string header, string data)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary["deviceId"] = deviceId;
            var headers = header.Split(',');
            var items = data.Split(',');
            for (int index = 0; index < headers.Length; index++)
            {
                if (!string.IsNullOrEmpty(items[index]))
                {
                    dictionary[headers[index]] = items[index];
                }
                if (headers[index] == "_ts" && string.IsNullOrEmpty(items[index]))
                {
                    dictionary[headers[index]] = DateTimeExtension.ConvertToUnixTimestamp().ToString();
                }
            }
            // add fallback key as timestampUnix
            if (!dictionary.ContainsKey("timestampUnix"))
            {
                dictionary["timestampUnix"] = dictionary["_ts"];
            }
            return JsonConvert.SerializeObject(dictionary);
        }
    }
}