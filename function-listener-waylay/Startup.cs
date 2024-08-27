using System;
using Function.Contant;
using Function.Service;
using Function.Service.Abstraction;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using AHI.Infrastructure.SharedKernel;
using Microsoft.Extensions.Logging;
using AHI.Infrastructure.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;

[assembly: FunctionsStartup(typeof(Broker.Function.Startup))]
namespace Broker.Function
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
        }

        public const string SERVICE_NAME = "broker-function-listener-waylay";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddRabbitMQ(SERVICE_NAME);
            builder.Services.AddHttpClient(HttpClientName.WAYLAY, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Waylay:Endpoint"]);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(configuration["Waylay:TokenType"], configuration["Waylay:Token"]);
            });
            builder.Services.AddHttpClient(HttpClientName.DEVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Device"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddLoggingService();
            builder.Services.AddRedisCache();
            builder.Services.AddMultiTenantService();
            builder.Services.AddSingleton<IDeviceService, DeviceService>();
            builder.Services.AddSingleton<IWaylayService, WaylayService>();

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
