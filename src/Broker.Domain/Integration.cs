using System;
using System.Collections.Generic;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class Integration : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; } = "AC";
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public IntegrationDetail Detail { get; set; }
        public Lookup Lookup { get; set; }
        public virtual ICollection<EntityTagDb> EntityTags { get; set; }

        public Integration()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            EntityTags ??= new List<EntityTagDb>();
        }
    }
}
