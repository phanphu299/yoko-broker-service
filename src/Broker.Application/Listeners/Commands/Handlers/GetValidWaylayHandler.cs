using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetValidWaylayHandler : IRequestHandler<GetValidWaylay, IEnumerable<WaylayDto>>
    {
        private readonly IWaylayService _service;
        public GetValidWaylayHandler(IWaylayService service)
        {
            _service = service;
        }

        public Task<IEnumerable<WaylayDto>> Handle(GetValidWaylay request, CancellationToken cancellationToken)
        {
            return _service.GetAllWaylayAsync(request, cancellationToken);
        }
    }
}
