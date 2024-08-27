using System;
using System.Threading;
using System.Threading.Tasks;
using CoAP;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;

namespace SimulateEMQXDevice
{
    public class SendMessage
    {
        private readonly IConfiguration _configuration;
        private EmqxConnectionString _connectionString;

        public SendMessage(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = new EmqxConnectionString(_configuration["ConnectionString"]);
        }

        public async Task RunSampleAsync()
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Sample execution cancellation requested; will exit.");
            };
            Console.WriteLine($"{DateTime.Now}> Press Control+C at any time to quit the sample.");

            bool.TryParse(_configuration["UsingMqtt"], out bool usingMqtt);
            int.TryParse(_configuration["DelayInMilliseconds"], out int delay);

            if (usingMqtt)
            {
                await MqttPublish(delay, cts.Token);
            }
            else
            {
                await CoapPublish(delay, cts.Token);
            }
        }

        private object GetPayload(
            int messageId,
            double currentTemperature,
            double currentHumidity,
            int random)
        {
            var payload = new
            {
                messageId = messageId++,
                temperature = currentTemperature,
                humidity = currentHumidity,
                deviceId = _configuration["DeviceId"],
                timestamp = ConvertToUnixTimestamp(),
                ack = (random % 2) == 0 ? false : true,
                Double = random,
                txt = random.ToString() + "txt",
                Int = random
            };

            //var payload = new
            //{
            //    messageId = messageId++,
            //    temperature = new double[,]
            //    {
            //         { ConvertToUnixTimestamp(), currentTemperature },
            //         { ConvertToUnixTimestamp(), ++currentTemperature }
            //    },
            //    humidity = new double[,]
            //    {
            //         { ConvertToUnixTimestamp(), currentHumidity },
            //         { ConvertToUnixTimestamp(), ++currentHumidity }
            //    },
            //    deviceId = _configuration["DeviceId"],
            //    timestamp = ConvertToUnixTimestamp(),
            //    ack = (random % 2) == 0 ? false : true,
            //    snr = random,
            //    txt = random.ToString() + "txt"
            //};

            return payload;
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

        private async Task CoapPublish(int delayInMilliseconds, CancellationToken cancellationToken)
        {
            var clientId = Guid.NewGuid();
            var token = await GetCoapToken(clientId);

            var topic = _configuration["Coap:TelemetryTopic"];
            if (string.IsNullOrEmpty(topic))
                throw new Exception("CoapPublish: CoapTelemetryTopic is not configured");

            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();
            var delayed = 0;
            var errorCount = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (delayed != 0 && delayed < delayInMilliseconds)
                {
                    await Task.Delay(1000);
                    delayed += 1000;
                    continue;
                }
                delayed = 1;
                var postClient = new CoapClient(new Uri($"coap://{_connectionString.Server}/ps/coap/{topic}?clientid={clientId}&token={token}&qos=1"));
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;
                int random = rand.Next(1, 10);
                var payload = GetPayload(messageId, currentTemperature, currentHumidity, random);
                var codeString = string.Empty;
                try
                {
                    var clientResponse = postClient.Post(JsonConvert.SerializeObject(payload));
                    codeString = clientResponse.CodeString;
                    errorCount = 0;
                }
                catch
                {
                    clientId = Guid.NewGuid();
                    token = await GetCoapToken(clientId);
                    errorCount++;
                    Console.WriteLine($"\nTry get new token, time:{errorCount}");
                    if (errorCount > 2)
                    {
                        throw;
                    }
                    delayed = 0;
                    continue;
                }
                Console.WriteLine($"\nSent topic:{topic}");
                Console.WriteLine($"Sent payload: {JsonConvert.SerializeObject(payload)} \nStatus code: {codeString}");
            }
        }

        private async Task<string> GetCoapToken(Guid clientId)
        {
            var client = new CoapClient(new Uri($"coap://{_connectionString.Server}/mqtt/connection?clientid={clientId}&username={_connectionString.Username}&password={_connectionString.Password}"));
            var response = client.Post("");
            var token = response.ResponseText;
            Console.WriteLine($"Connection token {token}");
            return token;
        }

        private async Task MqttPublish(int delayInMilliseconds, CancellationToken cancellationToken)
        {
            var mqttFactory = new MqttFactory();

            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(_connectionString.Server)
                    .WithCredentials(_connectionString.Username, _connectionString.Password)
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                double minTemperature = 20;
                double minHumidity = 60;
                int messageId = 1;
                Random rand = new Random();
                var topic = _configuration["Mqtt:TelemetryTopic"];
                if (string.IsNullOrEmpty(topic))
                    throw new Exception("MqttPublish: MqttTelemetryTopic is not configured");

                while (!cancellationToken.IsCancellationRequested)
                {
                    double currentTemperature = minTemperature + rand.NextDouble() * 15;
                    double currentHumidity = minHumidity + rand.NextDouble() * 20;
                    int random = rand.Next(1, 10);
                    var payload = GetPayload(messageId, currentTemperature, currentHumidity, random);

                    var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(JsonConvert.SerializeObject(payload))
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                    var res = await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                    Console.WriteLine($"Sent payload: {JsonConvert.SerializeObject(payload)}");

                    Thread.Sleep(delayInMilliseconds);
                }

                await mqttClient.DisconnectAsync();

            }
        }
    }
}