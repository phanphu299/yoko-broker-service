using System;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class SchemaDetailDataOption : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public Guid SchemaDetailId { get; set; }
        public int Order { get; set; }
        public SchemaDetail SchemaDetail { get; set; }
        public SchemaDetailDataOption()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
        }
    }
}
