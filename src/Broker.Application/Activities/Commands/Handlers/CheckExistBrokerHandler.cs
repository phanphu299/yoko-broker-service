using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class CheckExistBrokerHandler : IRequestHandler<CheckExistBroker, BaseResponse>
    {
        private readonly IBrokerService _service;
        public CheckExistBrokerHandler(IBrokerService service)
        {
            _service = service;
        }

        public Task<BaseResponse> Handle(CheckExistBroker request, CancellationToken cancellationToken)
        {
            return _service.CheckExistBrokersAsync(request, cancellationToken);
        }
    }
}