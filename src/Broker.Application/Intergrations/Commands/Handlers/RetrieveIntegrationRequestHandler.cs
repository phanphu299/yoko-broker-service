using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command.Handler
{
    public class RetrieveIntegrationRequestHandler : IRequestHandler<RetrieveIntegration, IDictionary<string, object>>
    {
        private readonly IIntegrationService _service;
        public RetrieveIntegrationRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IDictionary<string, object>> Handle(RetrieveIntegration request, CancellationToken cancellationToken)
        {
            return _service.RetrieveAsync(request, cancellationToken);
        }
    }
}
