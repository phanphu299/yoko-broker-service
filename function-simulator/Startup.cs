using System;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Extension;
using AHI.Infrastructure.Cache.Redis.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.MultiTenancy.Http.Handler;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Broker.Function.Startup))]
namespace Broker.Function
{
    public class Startup : FunctionsStartup
    {
        public const string SERVICE_NAME = "data-simulation";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddRedisCache();
            builder.Services.AddMultiTenantService();
            builder.Services.AddRabbitMQ(SERVICE_NAME);
            builder.Services.AddHttpClient(HttpClientNames.MASTER_FUNCTION, (service, client) =>
             {
                 var configuration = service.GetRequiredService<IConfiguration>();
                 client.BaseAddress = new Uri(configuration["Function:Master"]);
             }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddHttpClient(HttpClientNames.BROKER_SERVICE, (service, client) =>
             {
                 var configuration = service.GetRequiredService<IConfiguration>();
                 client.BaseAddress = new Uri(configuration["Api:Broker"]);
             }).AddHttpMessageHandler<ClientCrendetialAuthentication>();
            builder.Services.AddScoped<IJobProcessing, JobProcessing>();
        }
    }
}
