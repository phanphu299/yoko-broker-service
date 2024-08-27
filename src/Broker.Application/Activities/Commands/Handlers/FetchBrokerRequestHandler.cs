using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class FetchBrokerRequestHandler : IRequestHandler<FetchBroker, BrokerDto>
    {
        private readonly IBrokerService _service;
        public FetchBrokerRequestHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BrokerDto> Handle(FetchBroker request, CancellationToken cancellationToken)
        {
            return _service.FetchAsync(request.Id);
        }
    }
}