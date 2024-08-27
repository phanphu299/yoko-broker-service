using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace AHI.Broker.Function.Trigger.EventHub
{
    public class GenerateRabbitmqMessage
    {

        [FunctionName("GenerateRabbitMQTestMessage")]
        public async Task<IActionResult> GenerateRabbitMQTestMessageAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/rabbitmq/load")] HttpRequestMessage req,
        [RabbitMQ(ConnectionStringSetting = "RabbitMQ")] IModel client,
        ILogger logger)
        {
            var payload = await req.Content.ReadAsStringAsync();
            client.BasicPublish(exchange: "ingestion-exchange",
                                routingKey: "all",
                                body: Encoding.UTF8.GetBytes(payload));
            return new OkObjectResult(new { IsSuccess = true });
        }
        [FunctionName("GenerateBrokerTestMessage")]
        public async Task<IActionResult> GenerateBrokerTestMessageAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/eventhub/load")] HttpRequestMessage req,
        ILogger logger)
        {
            var payload = await req.Content.ReadAsStringAsync();
            var request = JsonConvert.DeserializeObject<EventHubRequest>(payload);
            EventHubProducerClient client = new EventHubProducerClient(request.ConnectionString, request.EventHubName);
            EventDataBatch eventBatch = await client.CreateBatchAsync();
            eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(payload)));
            await client.SendAsync(eventBatch);
            return new OkObjectResult(new { IsSuccess = true });
        }
    }
    internal class EventHubRequest
    {
        public string ConnectionString { get; set; }
        public string EventHubName { get; set; }

    }
}
