using Broker.Application.Handler.Command.Model;
using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class FindBrokerById : IRequest<BrokerDto>
    {
        public Guid Id { get; set; }
        public FindBrokerById(Guid id)
        {
            Id = id;
        }
    }
}
