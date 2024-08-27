using System;
using System.Collections.Generic;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class Schema : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public ICollection<SchemaDetail> Details { get; private set; }
        public Schema()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
            Details = new List<SchemaDetail>();
        }
    }
}
