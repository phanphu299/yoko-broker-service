using System;
using System.Threading.Tasks;
using CommandLine;
using IoTDeviceSimulator.Device;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;

namespace IoTDeviceSimulator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var primaryConnectionString = configuration.GetSection("PrimaryConnectionString").Value;
            double seconds = configuration.GetSection("RunningTime").Value == null ? 0 : double.Parse(configuration.GetSection("RunningTime").Value);
            TimeSpan? appRunTime = seconds == 0 ? (TimeSpan?)null : TimeSpan.FromSeconds(seconds);
            using var deviceClient = DeviceClient.CreateFromConnectionString(
                primaryConnectionString,
                TransportType.Mqtt);

            var receiveSample = new MessageReceiveSample(deviceClient, TransportType.Mqtt, appRunTime);
            var sendSampleMessage = new SendMessageSample(deviceClient, TransportType.Mqtt, appRunTime, configuration);
            var task = new Task[]{
             receiveSample.RunSampleAsync(),
             sendSampleMessage.RunSampleAsync()
        };
            await Task.WhenAll(task);
            Console.WriteLine("Done.");
        }
    }
}
