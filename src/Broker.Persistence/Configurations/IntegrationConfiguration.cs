using Broker.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class IntegrationConfiguration : IEntityTypeConfiguration<Domain.Entity.Integration>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.Integration> builder)
        {
            // configure the model.
            builder.ToTable("integrations");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.HasOne(x => x.Detail).WithOne(x => x.Integration).HasForeignKey<IntegrationDetail>(x => x.IntegrationId);
            builder.HasOne(x => x.Lookup).WithMany(x => x.Integrations).HasForeignKey(x => x.Type);
            builder.HasMany(x => x.EntityTags).WithOne(x => x.Integration).HasForeignKey(x => x.EntityIdGuid).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
