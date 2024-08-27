using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;
using System.Collections.Generic;

namespace Broker.Application.Handler.Command.Handler
{
    public class ArchiveBrokerRequestHandler : IRequestHandler<ArchiveBroker, IEnumerable<ArchiveBrokerDto>>
    {
        private readonly IBrokerService _service;
        public ArchiveBrokerRequestHandler(IBrokerService service)
        {
            _service = service;
        }

        public async Task<IEnumerable<ArchiveBrokerDto>> Handle(ArchiveBroker request, CancellationToken cancellationToken)
        {
            return await _service.ArchiveAsync(request, cancellationToken);
        }
    }
}
