using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class GetSchemaByTypeHandler : IRequestHandler<GetSchemaByType, IntegrationSchemaDto>
    {
        private readonly ISchemaService _service;
        public GetSchemaByTypeHandler(ISchemaService service)
        {
            _service = service;
        }

        public Task<IntegrationSchemaDto> Handle(GetSchemaByType request, CancellationToken cancellationToken)
        {
            return _service.FindByTypeAsync(request, cancellationToken);
        }
    }
}
