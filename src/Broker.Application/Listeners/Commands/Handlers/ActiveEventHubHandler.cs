using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class TrackEventHubHandler : IRequestHandler<ActiveListener, bool>
    {
        private readonly IListenerService _service;
        public TrackEventHubHandler(IListenerService service)
        {
            _service = service;
        }

        public Task<bool> Handle(ActiveListener request, CancellationToken cancellationToken)
        {
            return _service.ActiveAsync(request.Id, cancellationToken);
        }
    }
}
