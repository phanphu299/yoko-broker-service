using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class AddBrokerHandler : IRequestHandler<AddBroker, BrokerDto>
    {
        private readonly IBrokerService _service;
        public AddBrokerHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BrokerDto> Handle(AddBroker request, CancellationToken cancellationToken)
        {
            return _service.AddBrokerAsync(request, cancellationToken);
        }
    }
}
