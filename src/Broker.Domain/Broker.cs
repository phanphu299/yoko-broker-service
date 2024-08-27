using System;
using System.Collections.Generic;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class Broker : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } = "IN";
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public BrokerDetail Detail { get; set; }
        public Lookup Lookup { get; set; }
        public bool IsShared { get; set; }
        public string ResourcePath { get; set; }
        public string CreatedBy { get; set; }

        public ICollection<BrokerTopic> Topics { get; set; }
        public virtual ICollection<EntityTagDb> EntityTags { get; set; }
        public Broker()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            Topics = new List<BrokerTopic>();
            EntityTags ??= new List<EntityTagDb>();
        }
    }
}
