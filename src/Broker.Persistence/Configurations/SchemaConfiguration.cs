using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class SchemaConfiguration : IEntityTypeConfiguration<Domain.Entity.Schema>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.Schema> builder)
        {
            // configure the model.
            builder.ToTable("schemas");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Name).HasColumnName("name");
            builder.HasMany(x => x.Details).WithOne(x => x.Schema).HasForeignKey(x => x.SchemaId);
        }
    }
}
