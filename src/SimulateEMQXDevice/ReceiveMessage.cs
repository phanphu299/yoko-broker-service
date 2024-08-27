using System;
using System.Threading;
using System.Threading.Tasks;
using CoAP;
using CoAP.Util;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using Action = System.Action;

namespace SimulateEMQXDevice
{
    public class ReceiveMessage
    {
        private readonly IConfiguration _configuration;
        private EmqxConnectionString _connectionString;

        public ReceiveMessage(IConfiguration configuration)
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

            string res = string.Empty;
            if (usingMqtt)
            {
                await MqttSubscribe(cts.Token);
            }
            else
            {
                CoapSubscribe(cts.Token);
            }
        }

        private async Task MqttSubscribe(CancellationToken cancellationToken)
        {
            var mqttFactory = new MqttFactory();
            using (var mqttClient = mqttFactory.CreateMqttClient())
            {

                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(_connectionString.Server)
                    .WithCredentials(_connectionString.Username, _connectionString.Password)
                    .WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
                    .Build();

                mqttClient.ApplicationMessageReceivedAsync += e =>
                {
                    Console.WriteLine($"Received application message. - {System.Text.Encoding.Default.GetString(e.ApplicationMessage.Payload)}");
                    return Task.CompletedTask;
                };

                await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

                var projectIdString = _configuration["ProjectId"];
                Guid.TryParse(projectIdString, out var projectId);
                var deviceId = _configuration["DeviceId"];
                var topic = _configuration["Mqtt:CommandTopic"];
                if(string.IsNullOrEmpty(topic))
                    topic = BrokerHelper.GenerateCommandTopic(projectId, deviceId);

                var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                    .WithTopicFilter(f => { f.WithTopic(topic); })
                    .Build();

                await mqttClient.SubscribeAsync(mqttSubscribeOptions, cancellationToken);

                Console.WriteLine("MQTT client subscribed to topic.");
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                }
            }
        }

        private void CoapSubscribe(CancellationToken cancellationToken)
        {
            string clientId = Guid.NewGuid().ToString();

            var client = new CoapClient(new Uri($"coap://{_connectionString.Server}/mqtt/connection?clientid={clientId}&username={_connectionString.Username}&password={_connectionString.Password}"));
            var responseMessage = client.Post("");
            var token = responseMessage.ResponseText;

            var projectIdString = _configuration["ProjectId"];
            Guid.TryParse(projectIdString, out var projectId);
            var deviceId = _configuration["DeviceId"];

            var topic = _configuration["Coap:CommandTopic"];
            if (string.IsNullOrEmpty(topic))
                throw new Exception("CoapSubscribe: CoapCommandTopic is not configured");

            var heartbeat = Request.NewPut();
            heartbeat.URI = new Uri($"coap://{_connectionString.Server}/mqtt/connection?clientid={clientId}&token={token}");
            RecurringTask(() => Timerhandler(heartbeat), 10, cancellationToken);
            Console.WriteLine($"Subscribe topic : {topic}");
            Request request = Request.NewGet();
            request.URI = new Uri($"coap://{_connectionString.Server}/ps/coap/{topic}?clientid={clientId}&token={token}");
            Console.WriteLine(Utils.ToString(request));
            Listen(request, clientId, token);
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private void RecurringTask(Action action, int seconds, CancellationToken token)
        {
            if (action == null)
                return;
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    action();
                    await Task.Delay(TimeSpan.FromSeconds(seconds), token);
                }
            }, token);
        }

        private void Timerhandler(Request heartbeat)
        {
            heartbeat.Send();
            Console.WriteLine("Sent heartbeat");
        }

        private void Listen(Request request, string clientId, string token)
        {
            try
            {
                request.MarkObserve();
                request.Send();

                request.Respond += (o, e) =>
                {
                    // success
                    Response response = e.Response;
                    Console.WriteLine(Utils.ToString(response));
                };

                request.TimedOut += (o, e) =>
                {
                    // timeout
                    var heartbeat = Request.NewPut();
                    heartbeat.URI = new Uri($"coap://{_connectionString.Server}/mqtt/connection?clientid={clientId}&token={token}");
                    heartbeat.Send();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed executing request: " + ex.Message);
                Console.WriteLine(ex);
                Environment.Exit(1);
            }
        }
    }
}