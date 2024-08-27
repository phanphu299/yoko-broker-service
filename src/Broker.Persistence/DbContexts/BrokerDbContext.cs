using AHI.Infrastructure.Service.Tag.SqlServer.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Broker.Persistence.DbContexts
{
    public class BrokerDbContext : DbContext
    {
        public DbSet<Domain.Entity.Broker> Brokers { get; set; }
        public DbSet<Domain.Entity.BrokerDetail> BrokerDetails { get; set; }
        public DbSet<Domain.Entity.BrokerTopic> BrokerTopics { get; set; }
        public DbSet<Domain.Entity.Integration> Integrations { get; set; }
        public DbSet<Domain.Entity.IntegrationDetail> IntegrationDetails { get; set; }
        public DbSet<Domain.Entity.Schema> IntegrationSchemas { get; set; }
        public DbSet<Domain.Entity.Lookup> Lookups { get; set; }
        public DbSet<Domain.Entity.EntityTagDb> EntityTags { get; set; }
        public BrokerDbContext(DbContextOptions<BrokerDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrokerDbContext).Assembly);
            modelBuilder.ApplyConfiguration(new EntityTagConfiguration<Domain.Entity.EntityTagDb>());
        }
    }
}
