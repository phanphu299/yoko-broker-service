using AHI.Infrastructure.Service.Tag.Model;

namespace Broker.Domain.Entity
{
    public class EntityTagDb : EntityTag
    {
        public Broker Broker { get; set; }
        public Integration Integration { get; set; }
    }
}