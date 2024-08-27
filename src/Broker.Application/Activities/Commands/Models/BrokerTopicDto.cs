using System;
using System.Linq.Expressions;

namespace Broker.Application.Handler.Command.Model
{
    public class BrokerTopicDto
    {
        public Guid BrokerId { get; set; }
        public Guid ClientId { get; set; }
        public string AccessToken { get; set; }
        public string Topic { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public static Expression<Func<Domain.Entity.BrokerTopic, BrokerTopicDto>> Projection
        {
            get
            {
                return entity => new BrokerTopicDto
                {
                    BrokerId = entity.BrokerId,
                    Topic = entity.Topic,
                    CreatedUtc = entity.CreatedUtc,
                    UpdatedUtc = entity.UpdatedUtc,
                    ClientId = entity.ClientId,
                    AccessToken = entity.AccessToken
                };
            }
        }

        public static BrokerTopicDto Create(Domain.Entity.BrokerTopic entity)
        {
            if (entity == null)
                return null;
            return Projection.Compile().Invoke(entity);
        }
    }
}
