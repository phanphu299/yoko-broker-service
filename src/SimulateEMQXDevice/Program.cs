using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SimulateEMQXDevice
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                   .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();
            var receiveSample = new ReceiveMessage(config);
            var sendSampleMessage = new SendMessage(config);

            bool.TryParse(config["SendMessage"], out bool isPublishing);
            bool.TryParse(config["UsingMqtt"], out bool usingMqtt);
            if (usingMqtt)
            {
                var task = new Task[]
                {
                    receiveSample.RunSampleAsync(),
                    sendSampleMessage.RunSampleAsync()
                };
                await Task.WhenAll(task);
            }
            else
            {
                if (isPublishing)
                {
                    await sendSampleMessage.RunSampleAsync();
                }
                else
                {
                    await receiveSample.RunSampleAsync();
                }
            }
            
            Console.WriteLine("Done.");
        }
    }
}
