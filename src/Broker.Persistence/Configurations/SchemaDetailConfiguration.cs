using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class SchemaDetailConfiguration : IEntityTypeConfiguration<Domain.Entity.SchemaDetail>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.SchemaDetail> builder)
        {
            // configure the model.
            builder.ToTable("schema_details");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.SchemaId).HasColumnName("schema_id");
            builder.Property(x => x.Key).HasColumnName("key");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.Property(x => x.IsRequired).HasColumnName("is_required");
            builder.Property(x => x.PlaceHolder).HasColumnName("place_holder");
            builder.Property(x => x.DataType).HasColumnName("data_type");
            builder.Property(x => x.IsReadonly).HasColumnName("is_readonly");
            builder.Property(x => x.Regex).HasColumnName("regex");
            builder.Property(x => x.MinValue).HasColumnName("min_value");
            builder.Property(x => x.MaxValue).HasColumnName("max_value");
            builder.Property(x => x.DefaultValue).HasColumnName("default_value");
            builder.Property(x => x.Order).HasColumnName("order");
            builder.Property(x => x.DependOn).HasColumnName("depend_on_key");
            builder.Property(x => x.IsAllowCopy).HasColumnName("enable_copy");
            builder.Property(x => x.IsAllowEdit).HasColumnName("is_editable");
            builder.Property(x => x.Endpoint).HasColumnName("endpoint");
        }
    }
}
