using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class SchemaDetailDataOptionConfiguration : IEntityTypeConfiguration<Domain.Entity.SchemaDetailDataOption>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.SchemaDetailDataOption> builder)
        {
            // configure the model.
            builder.ToTable("schema_detail_options");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.Id).HasColumnName("code");
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.SchemaDetailId).HasColumnName("schema_detail_id");
            builder.HasOne(x => x.SchemaDetail).WithMany(x => x.Options).HasForeignKey(x => x.SchemaDetailId);
            builder.Property(x => x.Order).HasColumnName("order");
        }
    }
}
