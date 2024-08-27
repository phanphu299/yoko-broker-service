using Broker.Application.Handler.Command.Model;
using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class GetBrokerById : IRequest<BrokerDto>
    {
        public Guid Id { get; set; }

        public bool IncludeDeletedRecords { get; set; }

        public GetBrokerById(Guid id)
        {
            Id = id;
        }
    }
}
