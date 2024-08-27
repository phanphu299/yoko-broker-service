using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class CheckExistIntegrationRequestHandler : IRequestHandler<CheckExistIntegration, BaseResponse>
    {
        private readonly IIntegrationService _service;
        public CheckExistIntegrationRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(CheckExistIntegration request, CancellationToken cancellationToken)
        {
            return _service.CheckExistIntegrationsAsync(request, cancellationToken);
        }
    }
}