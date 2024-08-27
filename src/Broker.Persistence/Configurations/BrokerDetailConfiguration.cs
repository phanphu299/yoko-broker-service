using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class BrokerDetailConfiguration : IEntityTypeConfiguration<Domain.Entity.BrokerDetail>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.BrokerDetail> builder)
        {
            // configure the model.
            builder.ToTable("broker_details");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Content).HasColumnName("content");
            builder.Property(x => x.BrokerId).HasColumnName("broker_id");
        }
    }
}
