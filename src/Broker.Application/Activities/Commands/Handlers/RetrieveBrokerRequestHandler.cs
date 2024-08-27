using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command.Handler
{
    public class RetrieveBrokerRequestHandler : IRequestHandler<RetrieveBroker, IDictionary<string, object>>
    {
        private readonly IBrokerService _service;
        public RetrieveBrokerRequestHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<IDictionary<string, object>> Handle(RetrieveBroker request, CancellationToken cancellationToken)
        {
            return _service.RetrieveAsync(request, cancellationToken);
        }
    }
}
