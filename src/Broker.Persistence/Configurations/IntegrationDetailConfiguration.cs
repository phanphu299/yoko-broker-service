using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class IntegrationDetailConfiguration : IEntityTypeConfiguration<Domain.Entity.IntegrationDetail>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.IntegrationDetail> builder)
        {
            // configure the model.
            builder.ToTable("integration_details");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Content).HasColumnName("content");
            builder.Property(x => x.IntegrationId).HasColumnName("integration_id");
        }
    }
}
