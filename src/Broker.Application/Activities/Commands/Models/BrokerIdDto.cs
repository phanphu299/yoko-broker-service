using System;

namespace Broker.Application.Handler.Command.Model
{
    public class BrokerIdDto
    {
        public BrokerIdDto(Guid oldId, Guid newId)
        {
            OldId = oldId;
            NewId = newId;
        }

        public Guid OldId { get; set; }
        public Guid NewId { get; set; }
    }
}
