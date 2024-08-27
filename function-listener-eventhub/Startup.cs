using System;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using AHI.Infrastructure.OpenTelemetry;
using AHI.Infrastructure.SharedKernel;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service;
using AHI.Broker.Function.Service.Abstraction;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using AHI.Infrastructure.Bus.Kafka.Model;

[assembly: FunctionsStartup(typeof(Broker.Function.Startup))]
namespace Broker.Function
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
        }

        public const string SERVICE_NAME = "broker-function-listener-eventhub";
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenantService();
            builder.Services.AddRedisCache();
            builder.Services.AddMemoryCache();
            builder.Services.AddLoggingService();

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

            builder.Services.AddHttpClient(HttpClientNames.MASTER_FUNCTION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Function:Master"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddScoped<IMasterService, MasterService>();
        }
    }
}
