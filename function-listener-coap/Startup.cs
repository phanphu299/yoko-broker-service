using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.OpenTelemetry;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using AHI.Infrastructure.SharedKernel;
using AHI.Infrastructure.Bus.Kafka.Model;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Broker.Function.Startup))]
namespace Broker.Function
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
        }
        public const string SERVICE_NAME = "listener-coap";
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            var kafkaOption = configuration.GetSection(nameof(KafkaOption)).Get<KafkaOption>();
            if (kafkaOption?.Enabled == true)
            {
                builder.Services.AddKafka();
            }
            else
            {
                builder.Services.AddRabbitMQ(SERVICE_NAME);
            }

            builder.Services.AddMemoryCache();
            builder.Services.AddLoggingService();
            builder.Services.AddOtelTracingService(SERVICE_NAME, typeof(Startup).Assembly.GetName().Version.ToString());
            builder.Services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(option =>
                {
                    option.SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(SERVICE_NAME, typeof(Startup).Assembly.GetName().Version.ToString()));
                    option.AddOtlpExporter(oltp =>
                    {
                        oltp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
                });
            });
        }
    }
}
