using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;
using AHI.Infrastructure.SharedKernel.Model;
using Broker.Application.Service.Abstractions;

namespace Broker.Application.Handler.Command.Handler
{
    public class SearchBrokerRequestHandler : IRequestHandler<SearchBroker, BaseSearchResponse<BrokerDto>>
    {
        private readonly IBrokerService _service;
        public SearchBrokerRequestHandler(IBrokerService service)
        {
            _service = service;
        }

        public async Task<BaseSearchResponse<BrokerDto>> Handle(SearchBroker request, CancellationToken cancellationToken)
        {
            // if (request.ClientOverride == false)
            // {
            //     var pageSize = await _systemContext.GetValueAsync(DefaultSearchConstants.DEFAULT_SEARCH_PAGE_SIZE, DefaultSearchConstants.DEFAULT_VALUE_PAGE_SIZE);
            //     request.PageSize = System.Convert.ToInt32(pageSize);
            // }
            return await _service.SearchAsync(request);
        }
    }
}
