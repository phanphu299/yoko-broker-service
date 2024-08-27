using Broker.Application.Handler.Command.Model;
using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class GetIntegrationById : IRequest<IntegrationDto>
    {
        public Guid Id { get; set; }
        public GetIntegrationById(Guid id)
        {
            Id = id;
        }
    }
}
