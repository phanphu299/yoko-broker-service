using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Models;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class ExportBrokerRequestHandler : IRequestHandler<ExportBroker, ActivityResponse>
    {
        private readonly IBrokerService _service;
        public ExportBrokerRequestHandler(IBrokerService service)
        {
            _service = service;
        }
        public Task<ActivityResponse> Handle(ExportBroker request, CancellationToken cancellationToken)
        {
            return _service.ExportAsync(request, cancellationToken);
        }
    }
}
