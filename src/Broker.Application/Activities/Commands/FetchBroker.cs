using Broker.Application.Handler.Command.Model;
using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class FetchBroker : IRequest<BrokerDto>
    {
        public Guid Id { get; set; }

        public FetchBroker(Guid id)
        {
            Id = id;
        }
    }
}