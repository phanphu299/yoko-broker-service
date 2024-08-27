using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Reflection;
using Broker.Application.Service.Abstraction;
using Broker.Application.Service;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Service.Extension;
using Microsoft.Extensions.Configuration;
using Broker.Application.Handler.Command;
using Broker.Application.Service.Abstractions;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using System;
using Broker.Application.Broker.Validation;
using Broker.Application.Intergration.Validation;
using Broker.Application.Filters;
using Broker.Application.Constants;
using Broker.Application.Pipelines.ValidatorPipelines;
using Broker.Application.FileRequest.Command;
using Broker.Application.FileRequest.Validation;
using System.Collections.Generic;
using Configuration.Application.Constant;
using AHI.Infrastructure.OpenTelemetry;
using Prometheus;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using AHI.Infrastructure.Audit.Extension;
using Broker.Application.Constant;
using Broker.Application.Handler.Command.Model;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Enum;

namespace Broker.ApplicationExtension.Extension
{
    public static class ApplicationExtension
    {
        const string SERVICE_NAME = "broker-service";
        public static void AddApplicationServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddFrameworkServices();
            serviceCollection.AddApplicationValidator();
            serviceCollection.AddEntityTagService(DatabaseType.SqlServer);
            serviceCollection.AddMediatR(typeof(ApplicationExtension).GetTypeInfo().Assembly);
            serviceCollection.AddScoped<IBrokerService, BrokerService>();
            serviceCollection.AddScoped<IIntegrationService, IntegrationService>();
            serviceCollection.AddScoped<ISchemaService, SchemaService>();
            serviceCollection.AddScoped<IIntegrationValidator, EventHubIntegrationValidator>();
            serviceCollection.AddScoped<IBrokerValidator, BrokerValidator>();
            serviceCollection.AddScoped<IFileEventService, FileEventService>();
            serviceCollection.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            serviceCollection.AddMemoryCache();

            serviceCollection.AddScoped<IntegrationWaylay>();
            serviceCollection.AddScoped<IntegrationGreenKoncept>();
            serviceCollection.AddScoped<IDictionary<string, IIntegrationHandler>>(service =>
           {
               var dictionary = new Dictionary<string, IIntegrationHandler>();
               var waylayHandler = service.GetRequiredService<IntegrationWaylay>();
               var greenKonceptHandler = service.GetRequiredService<IntegrationGreenKoncept>();
               dictionary[IntegrationTypeConstants.WAY_LAY] = waylayHandler;
               dictionary[IntegrationTypeConstants.GREEN_KONCEPT] = greenKonceptHandler;
               return dictionary;

           });
            // using service bus
            serviceCollection.AddRabbitMQ(SERVICE_NAME);
            serviceCollection.AddHttpClient(HttpClientNames.CONFIGURATION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Configuration"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();

            serviceCollection.AddHttpClient(HttpClientNames.IDENTITY, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Authentication:Authority"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            serviceCollection.AddHttpClient(HttpClientNames.BROKER_FUNCTION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Function:Broker"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();

            serviceCollection.AddHttpClient(HttpClientNames.TAG_SERVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Tag"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();


            serviceCollection.AddHttpClient("device-service", (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Device"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>().UseHttpClientMetrics();

            serviceCollection.AddRedisCache();
            serviceCollection.AddScoped<ISystemContext, SystemContext>();

            serviceCollection.AddHttpClient(HttpClientNames.AZURE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Azure:Endpoint"] ?? "https://management.azure.com");
            }).AddHttpMessageHandler<AzureClientCrendetialAuthentication>();

            serviceCollection.AddHttpClient(HttpClientNames.AZURE_IDENTITY, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Azure:Authority"] ?? "https://login.microsoftonline.com");
            });
            serviceCollection.AddScoped<AzureClientCrendetialAuthentication>();
            serviceCollection.AddScoped<IEventHubService, EventHubService>();
            serviceCollection.AddScoped<IEmqxService, EmqxService>();
            serviceCollection.AddScoped<ILookupService, LookupService>();
            serviceCollection.AddScoped<IWaylayService, WaylayService>();
            serviceCollection.AddScoped<IGreenKonceptService, GreenKonceptService>();
            serviceCollection.AddScoped<IListenerService, ListenerService>();
            //serviceCollection.AddLoggingService();
            serviceCollection.AddAuditLogService();

            serviceCollection.AddScoped<CoapBrokerVerificationHandler>();
            serviceCollection.AddScoped<MqttBrokerVerificationHandler>();
            serviceCollection.AddScoped<EventHubBrokerVerificationHandler>();
            serviceCollection.AddScoped<IoTHubBrokerVerificationHandler>();
            serviceCollection.AddScoped<RestApiBrokerVerificationHandler>();
            serviceCollection.AddScoped<EventHubIntegrationVerificationHandler>();
            serviceCollection.AddScoped<WaylayIntegrationVerificationHandler>();
            serviceCollection.AddScoped<GreenKonceptIntegrationVerificationHandler>();

            serviceCollection.AddScoped<IDictionary<string, IContentVerificationHandler>>(service =>
            {
                var dictionary = new Dictionary<string, IContentVerificationHandler>();
                var coapBrokerVerifyHandler = service.GetRequiredService<CoapBrokerVerificationHandler>();
                var mqttBrokerVerifyHandler = service.GetRequiredService<MqttBrokerVerificationHandler>();
                var eventHubBrokerVerifyHandler = service.GetRequiredService<EventHubBrokerVerificationHandler>();
                var iotHubBrokerVerifyHandler = service.GetRequiredService<IoTHubBrokerVerificationHandler>();
                var restApiBrokerVerifyHandler = service.GetRequiredService<RestApiBrokerVerificationHandler>();
                var eventHubIntegrationVerifyHandler = service.GetRequiredService<EventHubIntegrationVerificationHandler>();
                var waylayIntegrationVerifyHandler = service.GetRequiredService<WaylayIntegrationVerificationHandler>();
                var greenKonceptIntegrationVerifyHandler = service.GetRequiredService<GreenKonceptIntegrationVerificationHandler>();

                dictionary[BrokerTypeConstants.EMQX_COAP] = coapBrokerVerifyHandler;
                dictionary[BrokerTypeConstants.EMQX_MQTT] = mqttBrokerVerifyHandler;
                dictionary[BrokerTypeConstants.EVENT_HUB] = eventHubBrokerVerifyHandler;
                dictionary[BrokerTypeConstants.IOT_HUB] = iotHubBrokerVerifyHandler;
                dictionary[BrokerTypeConstants.REST_API] = restApiBrokerVerifyHandler;
                dictionary[IntegrationTypeConstants.INTEGRATION_EVENT_HUB] = eventHubIntegrationVerifyHandler;
                dictionary[IntegrationTypeConstants.WAY_LAY] = waylayIntegrationVerifyHandler;
                dictionary[IntegrationTypeConstants.GREEN_KONCEPT] = greenKonceptIntegrationVerifyHandler;
                return dictionary;
            });

            serviceCollection.AddOtelTracingService(SERVICE_NAME, typeof(ApplicationExtension).Assembly.GetName().Version.ToString());
            // for production, no need to output to console.
            // will adapt with open telemetry collector in the future.
            serviceCollection.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(option =>
               {
                   option.SetResourceBuilder(
                   ResourceBuilder.CreateDefault()
                       .AddService(SERVICE_NAME, typeof(ApplicationExtension).Assembly.GetName().Version.ToString()));
                   // option.AddConsoleExporter();
                   option.AddOtlpExporter(oltp =>
                   {
                       oltp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                   });
               });
            });
        }

        public static void AddApplicationValidator(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<FluentValidation.IValidator<AddBroker>, AddBrokerValidation>();
            serviceCollection.AddSingleton<FluentValidation.IValidator<UpdateBroker>, UpdateBrokerValidation>();
            serviceCollection.AddSingleton<FluentValidation.IValidator<AddIntegration>, AddIntegrationValidation>();
            serviceCollection.AddSingleton<FluentValidation.IValidator<UpdateIntegration>, UpdateIntegrationValidation>();
            serviceCollection.AddSingleton<FluentValidation.IValidator<ImportFile>, ImportFileValidation>();
            serviceCollection.AddSingleton<FluentValidation.IValidator<ArchiveBrokerDto>, ArchiveBrokerDtoValidation>();
            serviceCollection.AddSingleton<FluentValidation.IValidator<ArchiveIntegrationDto>, ArchiveIntegrationDtoValidation>();
        }
    }
}
