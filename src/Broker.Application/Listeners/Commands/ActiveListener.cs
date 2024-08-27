using MediatR;
using System;

namespace Broker.Application.Handler.Command
{
    public class ActiveListener : IRequest<bool>
    {
        public Guid Id { get; private set; }
        public ActiveListener(Guid id)
        {
            Id = id;
        }

    }
}
