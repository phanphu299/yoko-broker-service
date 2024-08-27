using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class DeleteBrokerByIdHandler : IRequestHandler<DeleteBrokerById, BaseResponse>
    {
        private readonly IBrokerService _service;
        public DeleteBrokerByIdHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(DeleteBrokerById request, CancellationToken cancellationToken)
        {
            return _service.DeleteBrokerAsync(request, cancellationToken);
        }
    }
}
