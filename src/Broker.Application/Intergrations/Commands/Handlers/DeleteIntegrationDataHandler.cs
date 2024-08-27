using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class DeleteIntegrationDataHandler : IRequestHandler<DeleteIntegration, BaseResponse>
    {
        private readonly IIntegrationService _service;
        public DeleteIntegrationDataHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(DeleteIntegration request, CancellationToken cancellationToken)
        {
            return _service.RemoveAsync(request, cancellationToken);
        }
    }
}
