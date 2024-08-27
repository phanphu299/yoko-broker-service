using Broker.Application.Handler.Command.Model;
using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class FetchIntegration : IRequest<IntegrationDto>
    {
        public Guid Id { get; set; }
        public FetchIntegration(Guid id)
        {
            Id = id;
        }
    }
}