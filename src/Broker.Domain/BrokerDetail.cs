using System;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class BrokerDetail : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid BrokerId { get; set; }
        public Broker Broker { get; set; }
        public string Content { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public BrokerDetail()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
        }
    }
}
