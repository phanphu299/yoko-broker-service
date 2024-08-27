using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using MediatR;
using AHI.Infrastructure.SharedKernel.Model;
using Broker.Application.Service.Abstractions;

namespace Broker.Application.Handler.Command.Handler
{
    public class SearchIntegrationRequestHandler : IRequestHandler<SearchIntegration, BaseSearchResponse<IntegrationDto>>
    {
        private readonly IIntegrationService _service;
        public SearchIntegrationRequestHandler(IIntegrationService service)
        {
            _service = service;
        }

        public async Task<BaseSearchResponse<IntegrationDto>> Handle(SearchIntegration request, CancellationToken cancellationToken)
        {
            // //override the default page search
            // if (request.ClientOverride == false)
            // {
            //     var pageSize = await _systemContext.GetValueAsync(DefaultSearchConstants.DEFAULT_SEARCH_PAGE_SIZE, DefaultSearchConstants.DEFAULT_VALUE_PAGE_SIZE);
            //     request.PageSize = System.Convert.ToInt32(pageSize);
            // }
            return await _service.SearchAsync(request);
        }
    }
}
