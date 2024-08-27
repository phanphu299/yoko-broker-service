using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command.Handler
{
    public class ArchiveIntegrationRequestHandler : IRequestHandler<ArchiveIntegration, IEnumerable<ArchiveIntegrationDto>>
    {
        private readonly IIntegrationService _service;
        public ArchiveIntegrationRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IEnumerable<ArchiveIntegrationDto>> Handle(ArchiveIntegration request, CancellationToken cancellationToken)
        {
            return _service.ArchiveAsync(request, cancellationToken);
        }
    }
}
