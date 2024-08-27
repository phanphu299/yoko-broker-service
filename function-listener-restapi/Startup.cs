using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using AHI.Infrastructure.SharedKernel;
using Microsoft.Extensions.Logging;
using AHI.Infrastructure.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using AHI.Infrastructure.Bus.Kafka.Model;

[assembly: FunctionsStartup(typeof(AHI.Broker.Function.Startup))]
namespace AHI.Broker.Function
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
        }

        public const string SERVICE_NAME = "broker-function-listener-restapi";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenantService();
            var configuration = builder.GetContext().Configuration;
            var kafkaOption = configuration.GetSection(nameof(KafkaOption)).Get<KafkaOption>();
            if(kafkaOption?.Enabled == true)
            {                
                builder.Services.AddKafka();
            }
            else
            {
                builder.Services.AddRabbitMQ(SERVICE_NAME);
            }
            builder.Services.AddRedisCache();
            builder.Services.AddLoggingService();
            builder.Services.AddHttpClient(HttpClientNames.NOTIFICATION_HUB, (service, client) =>
             {
                 var configuration = service.GetRequiredService<IConfiguration>();
                 var endpoint = configuration["NotificationHubEndpoint"];
                 client.BaseAddress = new Uri(endpoint);
             }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(HttpClientNames.STORAGE, (service, client) =>
           {
               var configuration = service.GetRequiredService<IConfiguration>();
               client.BaseAddress = new Uri(configuration["Api:Storage"]);
           }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.DEVICE_FUNCTION, (service, client) =>
             {
                 var configuration = service.GetRequiredService<IConfiguration>();
                 client.BaseAddress = new Uri(configuration["Function:Device"]);
             }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.BROKER_SERVICE, (service, client) =>
             {
                 var configuration = service.GetRequiredService<IConfiguration>();
                 client.BaseAddress = new Uri(configuration["Api:Broker"]);
             }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddScoped<IStorageService, StorageService>();
            builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IDeviceService, DeviceService>();

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
