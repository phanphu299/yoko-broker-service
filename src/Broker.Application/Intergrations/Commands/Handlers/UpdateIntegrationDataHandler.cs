using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class UpdateIntegrationDataHandler : IRequestHandler<UpdateIntegration, IntegrationDto>
    {
        private readonly IIntegrationService _service;
        public UpdateIntegrationDataHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IntegrationDto> Handle(UpdateIntegration request, CancellationToken cancellationToken)
        {
            return _service.UpdateAsync(request, cancellationToken);
        }
    }
}
