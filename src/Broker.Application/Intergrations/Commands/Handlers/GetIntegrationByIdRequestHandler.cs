using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetIntegrationByIdRequestHandler : IRequestHandler<GetIntegrationById, IntegrationDto>
    {
        private readonly IIntegrationService _service;
        public GetIntegrationByIdRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IntegrationDto> Handle(GetIntegrationById request, CancellationToken cancellationToken)
        {
            return _service.FindByIdAsync(request, cancellationToken);
        }
    }
}
