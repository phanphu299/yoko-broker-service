using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetValidEventHubHandler : IRequestHandler<GetValidEventHub, IEnumerable<EventHubDto>>
    {
        private readonly IEventHubService _service;
        public GetValidEventHubHandler(IEventHubService service)
        {
            _service = service;
        }

        public Task<IEnumerable<EventHubDto>> Handle(GetValidEventHub request, CancellationToken cancellationToken)
        {
            return _service.GetAllEventHubAsync(request, cancellationToken);
        }
    }
}
