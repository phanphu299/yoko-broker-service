using Broker.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class BrokerConfiguration : IEntityTypeConfiguration<Domain.Entity.Broker>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.Broker> builder)
        {
            // configure the model.
            builder.ToTable("brokers");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.Status).HasColumnName("status");
            builder.Property(x => x.Type).HasColumnName("type");
            builder.Property(x => x.Deleted).HasColumnName("deleted");
            builder.Property(x => x.IsShared).HasColumnName("is_shared");
            builder.Property(x => x.ResourcePath).HasColumnName("resource_path");
            builder.Property(x => x.CreatedBy).HasColumnName("created_by");
            builder.HasOne(x => x.Detail).WithOne(x => x.Broker).HasForeignKey<BrokerDetail>(x => x.BrokerId);
            builder.HasOne(x => x.Lookup).WithMany(x => x.Brokers).HasForeignKey(x => x.Type);
            builder.HasMany(x => x.Topics).WithOne(x => x.Broker).HasForeignKey(x => x.BrokerId);
            builder.HasMany(x => x.EntityTags).WithOne(x => x.Broker).HasForeignKey(x => x.EntityIdGuid).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
