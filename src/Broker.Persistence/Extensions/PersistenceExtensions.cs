using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Broker.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Broker.Application.Repository.Abstraction;
using Broker.Persistence.Repository;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using Broker.Application.Repository;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.Service.Tag.Extension;

namespace Broker.Persistence.Extension
{
    public static class PersistenceExtensions
    {
        public static void AddPersistenceService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<BrokerDbContext>((service, option) =>
            {
                var configuration = service.GetService<IConfiguration>();
                var tenantContext = service.GetRequiredService<ITenantContext>();
                var connectionString = configuration["ConnectionStrings:Default"].BuildConnectionString(configuration, tenantContext.ProjectId);
                option.UseSqlServer(connectionString);

            });
            // add other services like repository, application services
            serviceCollection.AddScoped<IBrokerRepository, BrokerRepository>();
            serviceCollection.AddScoped<IIntegrationRepository, IntegrationRepository>();
            serviceCollection.AddScoped<ISchemaRepository, SchemaRepository>();
            serviceCollection.AddScoped<ILookupRepository, LookupRepository>();
            serviceCollection.AddEntityTagRepository(typeof(BrokerDbContext));
            serviceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
