using System.Collections.Generic;
using AHI.Infrastructure.Repository.Model.Generic;

namespace Broker.Domain.Entity
{
    public class Lookup : IEntity<string>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LookupTypeCode { get; set; }
        public bool Active { get; set; }
        public ICollection<Integration> Integrations { get; set; }
        public ICollection<Broker> Brokers { get; set; }
        public Lookup()
        {
            Active = true;
            Integrations = new List<Integration>();
            Brokers = new List<Broker>();
        }
    }
}
