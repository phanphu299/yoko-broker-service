using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetValidGreenKonceptHandler : IRequestHandler<GetValidGreenKoncept, IEnumerable<GreenKonceptDto>>
    {
        private readonly IGreenKonceptService _service;
        public GetValidGreenKonceptHandler(IGreenKonceptService service)
        {
            _service = service;
        }

        public Task<IEnumerable<GreenKonceptDto>> Handle(GetValidGreenKoncept request, CancellationToken cancellationToken)
        {
            return _service.GetAllGreenKonceptAsync(request, cancellationToken);
        }
    }
}
