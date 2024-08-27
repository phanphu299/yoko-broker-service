using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetValidCoapHandler : IRequestHandler<GetValidCoap, IEnumerable<CoapDto>>
    {
        private readonly IEmqxService _service;
        public GetValidCoapHandler(IEmqxService service)
        {
            _service = service;
        }

        public Task<IEnumerable<CoapDto>> Handle(GetValidCoap request, CancellationToken cancellationToken)
        {
            return _service.GetAllCoapAsync(request, cancellationToken);
        }
    }
}
