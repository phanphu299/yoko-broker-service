using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class FetchIntegrationDataHandler : IRequestHandler<FetchIntegrationData, BaseSearchResponse<FetchDataDto>>
    {
        private readonly IIntegrationService _service;
        public FetchIntegrationDataHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<BaseSearchResponse<FetchDataDto>> Handle(FetchIntegrationData request, CancellationToken cancellationToken)
        {
            return _service.FetchDataAsync(request, cancellationToken);
        }
    }
}
