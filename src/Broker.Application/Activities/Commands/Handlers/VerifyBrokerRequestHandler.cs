using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using MediatR;
using Broker.Application.Service.Abstractions;
using AHI.Infrastructure.SharedKernel.Model;

namespace Broker.Application.Handler.Command.Handler
{
    public class VerifyBrokerRequestHandler : IRequestHandler<VerifyBroker, BaseResponse>
    {
        private readonly IBrokerService _service;
        public VerifyBrokerRequestHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(VerifyBroker request, CancellationToken cancellationToken)
        {
            return _service.VerifyArchiveDataAsync(request, cancellationToken);
        }
    }
}
