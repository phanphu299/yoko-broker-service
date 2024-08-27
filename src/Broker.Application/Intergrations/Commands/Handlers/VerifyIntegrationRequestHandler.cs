using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class VerifyIntegrationRequestHandler : IRequestHandler<VerifyIntegration, BaseResponse>
    {
        private readonly IIntegrationService _service;
        public VerifyIntegrationRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(VerifyIntegration request, CancellationToken cancellationToken)
        {
            return _service.VerifyArchiveDataAsync(request, cancellationToken);
        }
    }
}
