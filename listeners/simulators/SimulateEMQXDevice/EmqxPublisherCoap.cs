using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CoAP;
using System.Text.Json;

// not working yet, need update
namespace SimulateEMQXDevice
{
    public class EmqxPublisherCoap : IEmqxPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly EmqxConnectionOption _emqxConnection;
        private readonly List<MqttCustomClient> _mqttClients = new();

        public EmqxPublisherCoap(IConfiguration configuration)
        {
            _configuration = configuration;
            _emqxConnection = _configuration.GetSection("Mqtt").Get<EmqxConnectionOption>();
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }


        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Parallel.ForEach(_mqttClients, async (client) =>
                {
                    await client.DisconnectAsync();
                });
                Console.WriteLine("Sample execution cancellation requested; will exit.");
            };
            Console.WriteLine($"{DateTime.Now}> Press Control+C at any time to quit the sample.");
            CoapPublish(cancellationToken);
        }

        private object GetPayload(
            int messageId,
            string deviceId,
            double currentTemperature,
            double currentHumidity,
            int random,
            bool useLargePayload = false)
        {
            if (!useLargePayload)
                return new
                {
                    messageId,
                    deviceId,
                    timestamp = ConvertToUnixTimestamp(),
                    temperature = currentTemperature,
                    humidity = currentHumidity,
                    ack = (random % 2) != 0,
                    snr = random,
                    txt = random.ToString() + "txt",
                    intValue = random
                };

            return new
            {
                messageId,
                deviceId,
                timestamp = ConvertToUnixTimestamp(),
                temperature = new double[,]
                {
                        { ConvertToUnixTimestamp(), currentTemperature },
                        { ConvertToUnixTimestamp(), ++currentTemperature }
                },
                humidity = new double[,]
                {
                        { ConvertToUnixTimestamp(), currentHumidity },
                        { ConvertToUnixTimestamp(), ++currentHumidity }
                },
                ack = (random % 2) != 0,
                snr = random,
                txt = random.ToString() + "txt",
                zvelocity = random,
                zvelocity2 = ++random,
                zvelocity3 = ++random,
                zvelocity4 = ++random,
                zvelocity5 = ++random,
            };
        }

        private long ConvertToUnixTimestamp()
        {
            // DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            // TimeSpan diff = date.ToUniversalTime() - origin;
            // return Math.Floor(diff.TotalMilliseconds);
            //return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // var date = DateTime.UtcNow;
            //DateTimeOffset dateTimeOffset = new DateTimeOffset();
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private void CoapPublish(CancellationToken cancellationToken)
        {
            _ = int.TryParse(_configuration["DelayInMilliseconds"], out int delayInMilliseconds);
            var projectIds = _configuration["ProjectIds"].Split(';');
            if (projectIds.Length == 0)
                throw new Exception("ProjectIds is not configured");

            var clientId = $"demo-coap-publisher_{Guid.NewGuid()}";
            var client = new CoapClient(new Uri($"coap://{_emqxConnection.Server}/mqtt/connection?clientid={clientId}&username={_emqxConnection.Username}&password={_emqxConnection.Password}"));
            var response = client.Post("");
            var token = response.ResponseText;
            Console.WriteLine($"Connection token {token}");


            var deviceId = ""; //todo: update
            var topic = _configuration["Coap:TelemetryTopic"];
            if (string.IsNullOrEmpty(topic))
                throw new Exception("CoapPublish: CoapTelemetryTopic is not configured");

            topic = topic.Replace("{projectId}", projectIds[0]).Replace("{deviceId}", deviceId);
            var postClient = new CoapClient(new Uri($"coap://{_emqxConnection.Server}/ps/coap/{topic}?clientid={clientId}&token={token}&qos=1"));

            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();

            Console.WriteLine($"Sending to topic: {topic}");
            while (!cancellationToken.IsCancellationRequested)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;
                int random = rand.Next(1, 10);
                var payload = GetPayload(messageId++, deviceId, currentTemperature, currentHumidity, random);

                var clientResponse = postClient.Post(JsonSerializer.Serialize(payload));
                Console.WriteLine($"Sent topic:{topic}");
                Console.WriteLine($"Sent payload: {JsonSerializer.Serialize(payload)} \nStatus code: {clientResponse.CodeString}");
                Thread.Sleep(delayInMilliseconds);
            }
        }


    }
}
