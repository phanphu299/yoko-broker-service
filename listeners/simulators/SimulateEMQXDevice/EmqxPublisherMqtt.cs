using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace SimulateEMQXDevice
{
    public class EmqxPublisherMqtt : IEmqxPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly EmqxConnectionOption _emqxConnection;
        private readonly List<MqttCustomClient> _mqttClients = new();
        public EmqxPublisherMqtt(IConfiguration configuration)
        {
            _configuration = configuration;
            _emqxConnection = _configuration.GetSection("Mqtt").Get<EmqxConnectionOption>();
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var clients = CreateMqttClients();
            _mqttClients.AddRange(clients);
            while (!cancellationToken.IsCancellationRequested)
            {
                Parallel.ForEach(_mqttClients, async (mqttClient) =>
                {
                    await mqttClient.ConnectAsync();
                    _ = mqttClient.StartBenchmarkAsync(cancellationToken);
                });
                await Task.Delay(TimeSpan.FromMilliseconds(-1), cancellationToken);
            }

        }

        public async Task StopAsync()
        {
            Console.WriteLine("Stoping....");

            await Parallel.ForEachAsync(_mqttClients, async (mqttClient, cancellationToken) =>
            {
                var message = await mqttClient.DisconnectAsync();
            });
            Console.WriteLine("Stopped...");

        }

        private IEnumerable<MqttCustomClient> CreateMqttClients()
        {
            var topic = _configuration["Mqtt:Topic"];
            if (string.IsNullOrEmpty(topic))
                throw new Exception("MqttPublish: MqttTelemetryTopic is not configured");

            var mqttFactory = new MqttFactory();
            var mqttClients = new List<MqttCustomClient>();
            var projectInfos = _configuration["ProjectIds"].Split(';');
            _ = bool.TryParse(_configuration["UseLargePayload"], out bool useLargePayload);
            _ = int.TryParse(_configuration["DelayInMilliseconds"], out int delayInMilliseconds);

            foreach (var projectId in projectInfos)
            {
                var deviceIds = ReadDeviceIds(projectId);
                foreach (var deviceId in deviceIds)
                {
                    var client = new MqttCustomClient(_emqxConnection, mqttFactory, topic, projectId, deviceId, useLargePayload, delayInMilliseconds);
                    mqttClients.Add(client);
                }
            }
            return mqttClients;
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

    class MqttCustomClient
    {
        public int MessageCount { get; private set; } = 0;
        private readonly string _topic;
        private readonly string _deviceId;
        private readonly bool _useLargePayload;
        private readonly int _delayInMilliseconds;
        private readonly double _minTemperature = 20;
        private readonly double _minHumidity = 60;
        private readonly Random _random = new();
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;

        public MqttCustomClient(EmqxConnectionOption connection, MqttFactory factory, string topic, string projectId, string deviceId, bool useLargePayload, int delayInMilliseconds)
        {
            _deviceId = deviceId;
            _useLargePayload = useLargePayload;
            _delayInMilliseconds = delayInMilliseconds >= 10 ? delayInMilliseconds : 1000;
            _topic = topic.Replace("{projectId}", projectId).Replace("{deviceId}", deviceId);
            _random = new Random();
            _mqttClient = factory.CreateMqttClient();
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(connection.Server)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithCredentials(connection.Username, connection.Password)
                .WithClientId($"{Guid.Parse(projectId):N}__{_deviceId}")
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
                .Build();
        }

        public async Task ConnectAsync()
        {
            try
            {
                // _mqttClient.DisconnectedAsync += async (e) =>
                // {
                //     Console.WriteLine($"The MQTT client {_mqttClient.Options.ClientId} reconnecting...");
                //     if (e.ClientWasConnected)
                //     {
                //         // Use the current options as the new options.
                //         await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
                //     }
                // };
                var response = await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
                Console.WriteLine($"The MQTT client {_mqttClient.Options.ClientId} is connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection Error: Device: {_mqttClient.Options.ClientId} *** {ex.Message}");
            }
        }

        public async Task<int> DisconnectAsync()
        {
            _mqttClient.DisconnectedAsync += null;
            await _mqttClient?.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
            return MessageCount;
        }

        public async Task StartBenchmarkAsync(CancellationToken cancellationToken)
        {
            if (!_mqttClient.IsConnected)
                return;

            while (!cancellationToken.IsCancellationRequested)
            {
                _ = Task.Run(async () =>
                {
                    var message = new MqttApplicationMessageBuilder()
                                         .WithTopic(_topic)
                                         .WithPayload(JsonSerializer.Serialize(GetPayload()))
                                         .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                                         .Build();
                    await PublishAsync(message, cancellationToken);
                }, cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(_delayInMilliseconds), cancellationToken);
            }
        }

        private async Task PublishAsync(MqttApplicationMessage message, CancellationToken cancellationToken)
        {
            await _mqttClient.PublishAsync(message, cancellationToken);
            Console.WriteLine($"Sent payload: {Encoding.UTF8.GetString(message.PayloadSegment)}");
        }

        private object GetPayload()
        {
            MessageCount++;
            var temperature = _minTemperature + _random.NextDouble() * 15;
            var humidity = _minHumidity + _random.NextDouble() * 20;
            var random = _random.Next(1, 10);

            return !_useLargePayload ?
                 new
                 {
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
