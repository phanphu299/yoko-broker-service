using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Service.Abstraction;
using Device.Domain.Entity;
using MediatR;

namespace Broker.Application.Handler.Command.Handler
{
    public class QueryIntegrationDataHandler : IRequestHandler<QueryIntegrationData, IEnumerable<TimeSeriesDto>>
    {
        private readonly IIntegrationService _service;
        public QueryIntegrationDataHandler(IIntegrationService service)
        {
            _service = service;
        }

        public Task<IEnumerable<TimeSeriesDto>> Handle(QueryIntegrationData request, CancellationToken cancellationToken)
        {
            return _service.QueryTimeSeriesDataAsync(request, cancellationToken);
        }
    }
}
