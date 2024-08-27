using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Broker.Persistence.Configurations
{
    public class BrokerTopicConfiguration : IEntityTypeConfiguration<Domain.Entity.BrokerTopic>
    {
        public void Configure(EntityTypeBuilder<Domain.Entity.BrokerTopic> builder)
        {
            // configure the model.
            builder.ToTable("emqx_topics");
            builder.HasQueryFilter(x => !x.Deleted);
            builder.Property(x => x.UpdatedUtc).HasColumnName("updated_utc");
            builder.Property(x => x.CreatedUtc).HasColumnName("created_utc");
            builder.Property(x => x.Topic).HasColumnName("topic_name");
            builder.Property(x => x.BrokerId).HasColumnName("broker_id");
            builder.Property(x => x.ClientId).HasColumnName("client_id");
            builder.Property(x => x.AccessToken).HasColumnName("access_token");
        }
    }
}
