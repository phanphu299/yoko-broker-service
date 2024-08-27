using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class AddIntegrationDataHandler : IRequestHandler<AddIntegration, IntegrationDto>
    {
        private readonly IIntegrationService _service;
        public AddIntegrationDataHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IntegrationDto> Handle(AddIntegration request, CancellationToken cancellationToken)
        {
            return _service.AddAsync(request, cancellationToken);
        }
    }
}
