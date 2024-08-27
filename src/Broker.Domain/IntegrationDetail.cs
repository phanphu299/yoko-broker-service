using System;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class IntegrationDetail : IEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid IntegrationId { get; set; }
        public Integration Integration { get; set; }
        public string Content { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public bool Deleted { set; get; }
        public IntegrationDetail()
        {
            CreatedUtc = DateTime.UtcNow;
            UpdatedUtc = DateTime.UtcNow;
        }
    }
}
