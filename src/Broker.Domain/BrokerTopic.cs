using System;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class BrokerTopic : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid BrokerId { get; set; }
        public Guid ClientId { get; set; }
        public string AccessToken { get; set; }
        public string Topic { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public Broker Broker { get; set; }
        public BrokerTopic()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
        }
    }
}
