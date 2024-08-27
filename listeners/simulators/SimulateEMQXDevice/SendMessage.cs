using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using CoAP;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace SimulateEMQXDevice
{
    public class SendMessage
    {
        private readonly IConfiguration _configuration;
        private readonly EmqxConnectionString _connectionString;
        private readonly List<MqttCustomClient> _mqttClients = new();

        public SendMessage(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = new EmqxConnectionString(_configuration["ConnectionString"]);
        }

        public async Task RunAsync()
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
            _ = bool.TryParse(_configuration["UsingMqtt"], out bool usingMqtt);
            if (usingMqtt)
            {
                await MqttPublish(cts.Token);
            }
            else
            {
                CoapPublish(cts.Token);
            }
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
            var client = new CoapClient(new Uri($"coap://{_connectionString.Server}/mqtt/connection?clientid={clientId}&username={_connectionString.Username}&password={_connectionString.Password}"));
            var response = client.Post("");
            var token = response.ResponseText;
            Console.WriteLine($"Connection token {token}");


            var deviceId = ""; //todo: update
            var topic = _configuration["Coap:TelemetryTopic"];
            if (string.IsNullOrEmpty(topic))
                throw new Exception("CoapPublish: CoapTelemetryTopic is not configured");

            topic = topic.Replace("{projectId}", projectIds[0]).Replace("{deviceId}", deviceId);
            var postClient = new CoapClient(new Uri($"coap://{_connectionString.Server}/ps/coap/{topic}?clientid={clientId}&token={token}&qos=1"));

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

                var clientResponse = postClient.Post(JsonConvert.SerializeObject(payload));
                Console.WriteLine($"Sent topic:{topic}");
                Console.WriteLine($"Sent payload: {JsonConvert.SerializeObject(payload)} \nStatus code: {clientResponse.CodeString}");
                Thread.Sleep(delayInMilliseconds);
            }
        }

        private IEnumerable<MqttCustomClient> CreateMqttClients()
        {
            _ = bool.TryParse(_configuration["UseLargePayload"], out bool useLargePayload);
            var topic = _configuration["Mqtt:TelemetryTopic"];
            if (string.IsNullOrEmpty(topic))
                throw new Exception("MqttPublish: MqttTelemetryTopic is not configured");

            var mqttFactory = new MqttFactory();
            var mqttClients = new List<MqttCustomClient>();
            var projectInfos = _configuration["ProjectIds"].Split(';');
            foreach (var projectId in projectInfos)
            {
                var deviceIds = ReadDeviceIds(projectId);
                foreach (var deviceId in deviceIds)
                {
                    Console.WriteLine($"connected {deviceId}");
                    var client = new MqttCustomClient(mqttFactory, topic, projectId, deviceId, useLargePayload);
                    mqttClients.Add(client);
                }
            }
            return mqttClients;
        }

        private async Task MqttPublish(CancellationToken cancellationToken)
        {
            _ = int.TryParse(_configuration["DelayInMilliseconds"], out int delayInMilliseconds);
            while (!cancellationToken.IsCancellationRequested)
            {
                var mqttClients = CreateMqttClients();
                Parallel.ForEach(mqttClients, async (mqttClient) =>
                {
                    await mqttClient.ConnectAsync(_connectionString);
                    _ = mqttClient.BenchmarkAsync(delayInMilliseconds, cancellationToken);
                });
                await Task.Delay(TimeSpan.FromMilliseconds(-1), cancellationToken);
            }
        }

        private static List<string> ReadDeviceIds(string projectId)
        {
            var ids = new List<string>();
            string line;
            try
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, "data", $"{projectId}.txt");
                var sr = new StreamReader(filePath);
                line = sr.ReadLine();
                while (line != null)
                {
                    ids.Add(line);
                    line = sr.ReadLine();
                }
                sr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            return ids;
        }
    }

    internal class MqttCustomClient
    {
        public int MessageCount { get; private set; } = 0;
        private readonly Random _random = new();
        private readonly IMqttClient _mqttClient;
        private readonly string _topic;
        private readonly bool _useLargePayload;
        private readonly string _deviceId;
        private readonly double _minTemperature = 20;
        private readonly double _minHumidity = 60;
        public MqttCustomClient(MqttFactory factory, string topic, string projectId, string deviceId, bool useLargePayload)
        {
            _deviceId = deviceId;
            _random = new Random();
            _mqttClient = factory.CreateMqttClient();
            _useLargePayload = useLargePayload;
            _topic = topic.Replace("{projectId}", projectId).Replace("{deviceId}", deviceId);
        }

        public async Task ConnectAsync(EmqxConnectionString connectionString)
        {
            var options = new MqttClientOptionsBuilder()
                                         .WithTcpServer(connectionString.Server)
                                         .WithProtocolVersion(MqttProtocolVersion.V500)
                                         .WithCredentials(connectionString.Username, connectionString.Password)
                                         .WithClientId(_deviceId)
                                         .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                                         .Build();
            try
            {
                var response = await _mqttClient.ConnectAsync(options, CancellationToken.None);
                Console.WriteLine($"The MQTT client {_deviceId} is connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: Device: {_mqttClient.Options.ClientId} *** {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            await _mqttClient?.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
        }

        public async Task PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken)
        {
            await _mqttClient.PublishAsync(message, cancellationToken);
            // Console.WriteLine($"Sent payload: {Encoding.UTF8.GetString(message.PayloadSegment)}");
        }

        public async Task BenchmarkAsync(int delayInMilliseconds, CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
                return;

            while (!cancellationToken.IsCancellationRequested)
            {
                _ = Task.Run(async () =>
                {
                    var message = new MqttApplicationMessageBuilder()
                                         .WithTopic(_topic)
                                         .WithPayload(JsonConvert.SerializeObject(GetPayload()))
                                         .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                         .Build();
                    await PublishAsync(message, cancellationToken);
                }, cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(delayInMilliseconds), cancellationToken);
            }
        }

        private object GetPayload()
        {
            var temperature = _minTemperature + _random.NextDouble() * 15;
            var humidity = _minHumidity + _random.NextDouble() * 20;
            var random = _random.Next(1, 10);

            return !_useLargePayload ?
                 new
                 {
                     messageId = ++MessageCount,
                     deviceId = _deviceId,
                     timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                     temperature,
                     humidity,
                     ack = (random % 2) != 0,
                     snr = random,
                     txt = random.ToString() + "txt",
                     intValue = random
                 }
                : new
                {
                    messageId = ++MessageCount,
                    deviceId = _deviceId,
                    timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                    temperature = new double[,]
                    {
                        { DateTimeOffset.Now.ToUnixTimeMilliseconds(), temperature },
                        { DateTimeOffset.Now.ToUnixTimeMilliseconds(), ++temperature }
                    },
                    humidity = new double[,]
                    {
                        { DateTimeOffset.Now.ToUnixTimeMilliseconds(), humidity },
                        { DateTimeOffset.Now.ToUnixTimeMilliseconds(), ++humidity }
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
    }
}
