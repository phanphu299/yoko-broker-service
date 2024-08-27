using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class LookupConfiguration : IEntityTypeConfiguration<Domain.Entity.Lookup>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.Lookup> builder)
        {
            // configure the model.
            builder.ToTable("lookups");
            builder.Property(x => x.Id).HasColumnName("code").IsRequired();
            builder.Property(x => x.Name).HasColumnName("name").IsRequired();
            builder.Property(x => x.LookupTypeCode).HasColumnName("lookup_type_code").IsRequired();
            builder.Property(x => x.Active).HasColumnName("active");
        }
    }
}
