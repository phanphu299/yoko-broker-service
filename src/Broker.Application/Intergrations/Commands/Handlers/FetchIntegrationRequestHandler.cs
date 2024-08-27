using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class FetchIntegrationRequestHandler : IRequestHandler<FetchIntegration, IntegrationDto>
    {
        private readonly IIntegrationService _service;
        public FetchIntegrationRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IntegrationDto> Handle(FetchIntegration request, CancellationToken cancellationToken)
        {
            return _service.FetchAsync(request.Id);
        }
    }
}