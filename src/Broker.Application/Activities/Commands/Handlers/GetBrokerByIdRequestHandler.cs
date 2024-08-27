using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetBrokerByIdRequestHandler : IRequestHandler<GetBrokerById, BrokerDto>
    {
        private readonly IBrokerService _service;
        public GetBrokerByIdRequestHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BrokerDto> Handle(GetBrokerById request, CancellationToken cancellationToken)
        {
            return _service.FindByIdAsync(request, cancellationToken);
        }
    }
}
