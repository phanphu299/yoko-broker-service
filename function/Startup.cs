using System;
using System.Collections.Generic;
using System.Net.Http;
using Function.Filter;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.FileParser.Extension;
using AHI.Broker.Function.Model.ImportModel;
using AHI.Broker.Function.Service;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Broker.Function.Service.FileImport;
using AHI.Broker.Function.Service.FileImport.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Import.Abstraction;
using AHI.Infrastructure.Repository;
using StorageService = AHI.Broker.Function.Service.StorageService;
using AHI.Infrastructure.Export.Extension;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Broker.Function.FileParser;
using AHI.Infrastructure.Repository.Abstraction;
using AHI.Broker.Function.DelegatingHandler;
using Microsoft.Extensions.Logging;
using AHI.Infrastructure.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using AHI.Broker.Function.Model;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.Audit.Extension;
using AHI.Infrastructure.Service.Tag.Extension;

[assembly: FunctionsStartup(typeof(Broker.Function.Startup))]
namespace Broker.Function
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
            System.Diagnostics.Activity.DefaultIdFormat = System.Diagnostics.ActivityIdFormat.W3C;
        }

        public const string SERVICE_NAME = "broker-function";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMemoryCache();
            builder.Services.AddRedisCache();
            builder.Services.AddScoped<IStorageService, StorageService>();
            builder.Services.AddHttpClient(HttpClientNames.STORAGE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Storage"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.MASTER, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Master"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.MASTER_FUNCTION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Function:Master"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.IDENTITY, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Authentication:Authority"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.IDENTITY_FUNCTION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Function:Identity"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            builder.Services.AddHttpClient(HttpClientNames.BROKER, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Broker"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(HttpClientNames.TENANT, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Tenant"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(HttpClientNames.AZURE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Azure:Endpoint"] ?? "https://management.azure.com");
            }).AddHttpMessageHandler<AzureClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(HttpClientNames.AZURE_IDENTITY, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Azure:Authority"] ?? "https://login.microsoftonline.com");
            });
            builder.Services.AddHttpClient(HttpClientNames.CONFIGURATION, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Configuration"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(HttpClientNames.DEVICE, (service, client) =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["Api:Device"]);
            }).AddHttpMessageHandler<ClientCrendetialAuthentication>();

            // download link should be fully qualified URL, don't need to setup BaseAddress
            // download client must not use ClientCrendetialAuthentication handler to avoid Authorization conflict when download from blob storage
            builder.Services.AddHttpClient(HttpClientNames.DOWNLOAD_CLIENT);

            builder.Services.AddSingleton<AzureConfiguration>(service =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                var section = configuration.GetSection("Azure");
                var azureConfiguration = new AzureConfiguration();
                section.Bind(azureConfiguration);
                return azureConfiguration;
            });

            builder.Services.AddScoped<ICloudProvider>(service =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                var tenantContext = service.GetRequiredService<ITenantContext>();
                var masterService = service.GetRequiredService<IMasterService>();
                var httpClientFactory = service.GetRequiredService<IHttpClientFactory>();
                var azureEventHubLogger = service.GetRequiredService<ILoggerAdapter<AzureEventHubCloudProvider>>();
                var azureIotHubLogger = service.GetRequiredService<ILoggerAdapter<AzureIoTHubCloudProvider>>();
                var notification = service.GetRequiredService<INotificationService>();
                var azureConfiguration = service.GetRequiredService<AzureConfiguration>();
                var azureEventHub = new AzureEventHubCloudProvider(null, tenantContext, httpClientFactory, azureEventHubLogger, notification, masterService, azureConfiguration);
                var azureIoTHub = new AzureIoTHubCloudProvider(azureEventHub, tenantContext, httpClientFactory, azureIotHubLogger, notification, masterService, azureConfiguration);
                return azureIoTHub;
            });

            builder.Services.AddScoped<ICloudDeviceRegistrationProvider>(service =>
            {
                var configuration = service.GetRequiredService<IConfiguration>();
                var tenantContext = service.GetRequiredService<ITenantContext>();
                var httpClientFactory = service.GetRequiredService<IHttpClientFactory>();
                var azureIoTHub = new AzureIoTDeviceRegistrationProvider(null, httpClientFactory, tenantContext);
                return azureIoTHub;
            });
            builder.Services.AddScoped<IBrokerRepository, BrokerRepository>();
            builder.Services.AddScoped<AzureClientCrendetialAuthentication>();
            builder.Services.AddScoped<ISystemContext, SystemContext>();

            builder.Services.AddExportingServices();
            builder.Services.AddDataParserServices();
            builder.Services.AddScoped<IFileHandler<BrokerModel>, BrokerJsonHandler>();
            builder.Services.AddScoped<IImportNotificationService, ImportNotificationService>();
            builder.Services.AddScoped<IExportNotificationService, ExportNotificationService>();
            builder.Services.AddScoped<IBrokerImportService, BrokerImportService>();
            builder.Services.AddScoped<IDeviceIotService, DeviceIotService>();
            builder.Services.AddScoped<ILookupService, LookupService>();
            builder.Services.AddScoped<IMasterService, MasterService>();
            builder.Services.AddScoped<IFileExportService, FileExportService>();
            builder.Services.AddScoped<IFileImportService, FileImportService>();
            builder.Services.AddScoped<IBrokerService, BrokerService>();
            builder.Services.AddScoped<IEntityTagService, EntityTagService>();
            builder.Services.AddScoped<BrokerExportHandler>();
            builder.Services.AddScoped<HttpMessageIotHandler>();

            builder.Services.AddScoped<IDictionary<string, IExportHandler>>(service =>
            {
                var dictionary = new Dictionary<string, IExportHandler>();
                var brokerTemplateExportHandler = service.GetRequiredService<BrokerExportHandler>();
                dictionary[IOEntityType.BROKER] = brokerTemplateExportHandler;
                return dictionary;
            });

            builder.Services.AddEntityTagService(AHI.Infrastructure.Service.Tag.Enum.DatabaseType.SqlServer);
            builder.Services.AddMultiTenantService();
            builder.Services.AddRabbitMQ(SERVICE_NAME);
            // builder.Services.AddApplicationInsightsTelemetry();
            // add import repository services
            builder.Services.AddScoped<BrokerRestApiValidator>();
            builder.Services.AddScoped<BrokerMqttValidator>();
            builder.Services.AddScoped<BrokerIoTHubValidator>();
            builder.Services.AddScoped<BrokerEventHubValidator>();
            builder.Services.AddScoped<IBrokerPersistenceValidator>(services =>
            {
                var restApiValidator = services.GetRequiredService<BrokerRestApiValidator>();
                var iotHubValidator = services.GetRequiredService<BrokerIoTHubValidator>();
                var eventHubValidator = services.GetRequiredService<BrokerEventHubValidator>();
                var mqttValidator = services.GetRequiredService<BrokerMqttValidator>();
                restApiValidator.SetNextValidator(mqttValidator);
                iotHubValidator.SetNextValidator(restApiValidator);
                eventHubValidator.SetNextValidator(iotHubValidator);
                return eventHubValidator;
            });

            builder.Services.AddScoped<IImportRepository<BrokerModel>, BrokerRepository>();

            builder.Services.AddScoped<IDictionary<Type, IFileImport>>(service =>
            {
                // return the proper type
                var deviceTemplate = service.GetRequiredService<IBrokerImportService>();
                return new Dictionary<Type, IFileImport>()
                {
                    {typeof(BrokerModel), deviceTemplate}
                };
            });
            // builder.Services.AddLoggingService();
            builder.Services.AddAuditLogService();
            builder.Services.AddNotification();

            builder.Services.AddOtelTracingService(SERVICE_NAME, typeof(Startup).Assembly.GetName().Version.ToString());
            builder.Services.AddLogging(builder =>
            {
                builder.AddOpenTelemetry(option =>
               {
                   option.SetResourceBuilder(
                   ResourceBuilder.CreateDefault()
                       .AddService(SERVICE_NAME, typeof(Startup).Assembly.GetName().Version.ToString()));
                   // option.AddConsoleExporter();
                   option.AddOtlpExporter(oltp =>
                   {
                       oltp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                   });
               });
            });
        }
    }
}
