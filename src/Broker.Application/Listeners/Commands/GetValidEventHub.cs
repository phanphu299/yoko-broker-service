using Broker.Application.Handler.Command.Model;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command
{
    public class GetValidEventHub : IRequest<IEnumerable<EventHubDto>>
    {
        public GetValidEventHub()
        {
        }
    }
}
