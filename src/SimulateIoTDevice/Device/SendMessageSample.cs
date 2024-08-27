using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
namespace IoTDeviceSimulator.Device
{
    public class SendMessageSample
    {
        private readonly TimeSpan? _maxRunTime;
        private readonly DeviceClient _deviceClient;
        private readonly TransportType _transportType;
        private readonly IConfiguration _configuration;

        public SendMessageSample(DeviceClient deviceClient, TransportType transportType, TimeSpan? maxRunTime, IConfiguration configuration)
        {
            _deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
            _transportType = transportType;
            _maxRunTime = maxRunTime;
            _configuration = configuration;
        }

        public async Task RunSampleAsync()
        {
            using var cts = _maxRunTime.HasValue
                ? new CancellationTokenSource(_maxRunTime.Value)
                : new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                Console.WriteLine("Sample execution cancellation requested; will exit.");
            };
            Console.WriteLine($"{DateTime.Now}> Press Control+C at any time to quit the sample.");

            // First receive C2D messages using the polling ReceiveAsync().
            Console.WriteLine($"\n{DateTime.Now}> Device waiting for C2D messages from the hub...");
            Console.WriteLine($"{DateTime.Now}> Use the Azure Portal IoT hub blade or Azure IoT Explorer to send a message to this device.");
            await SendDeviceToCloudMessagesAsync(cts.Token);
        }

        private async Task SendDeviceToCloudMessagesAsync(CancellationToken token)
        {
            double minTemperature = 20;
            double minHumidity = 60;
            int messageId = 1;
            Random rand = new Random();

            while (!token.IsCancellationRequested)
            {
                double currentTemperature = minTemperature + rand.NextDouble() * 15;
                double currentHumidity = minHumidity + rand.NextDouble() * 20;
                int random = rand.Next(1, 10);
                // var timestamp = ConvertToUnixTimestamp();

                // send message with single value
                var telemetryDataPoint = new
                {
                    messageId = messageId++,
                    temperature = currentTemperature,
                    humidity = currentHumidity,
                    deviceId = _configuration["DeviceId"],
                    timestamp = ConvertToUnixTimestamp(),
                    ack = (random % 2) == 0 ? false : true,
                    snr = random,
                    txt = random.ToString() + "txt"
                };

                // send message with multiple values
                // var telemetryDataPoint = new
                // {
                //     messageId = messageId++,
                //     temperature = new double[,]
                //     {
                //         { ConvertToUnixTimestamp(), currentTemperature },
                //         { ConvertToUnixTimestamp(), ++currentTemperature }
                //     },
                //     humidity = new double[,]
                //     {
                //         { ConvertToUnixTimestamp(), currentHumidity },
                //         { ConvertToUnixTimestamp(), ++currentHumidity }
                //     },
                //     deviceId = _configuration["DeviceId"],
                //     timestamp = ConvertToUnixTimestamp(),
                //     ack = (random % 2) == 0 ? false : true,
                //     snr = random,
                //     txt = random.ToString() + "txt"
                // };

                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("temperatureAlert", (currentTemperature > 30) ? "true" : "false");

                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                await Task.Delay(10000);
            }
        }
        private static long ConvertToUnixTimestamp()
        {
            // DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            // TimeSpan diff = date.ToUniversalTime() - origin;
            // return Math.Floor(diff.TotalMilliseconds);
            //return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            // var date = DateTime.UtcNow;
            //DateTimeOffset dateTimeOffset = new DateTimeOffset();
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}