using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class DeleteBrokerHandler : IRequestHandler<DeleteBroker, BaseResponse>
    {
        private readonly IBrokerService _service;
        public DeleteBrokerHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(DeleteBroker request, CancellationToken cancellationToken)
        {
            return _service.DeleteBrokersAsync(request, cancellationToken);
        }
    }
}
