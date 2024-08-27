using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class UpdateBrokerHandler : IRequestHandler<UpdateBroker, BrokerDto>
    {
        private readonly IBrokerService _service;
        public UpdateBrokerHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BrokerDto> Handle(UpdateBroker request, CancellationToken cancellationToken)
        {
            return _service.UpdateBrokerAsync(request, cancellationToken);
        }
    }
}
