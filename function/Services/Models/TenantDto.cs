using System;

namespace AHI.Broker.Function.Service.Model
{
    public class TenantDto
    {
        public string ResourceId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string Email { get; set; }
        public bool IsMigrated { get; set; }
        public bool Deleted { get; set; }
    }
}