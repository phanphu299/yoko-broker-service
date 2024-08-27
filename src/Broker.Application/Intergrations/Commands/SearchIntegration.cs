using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Constants;

namespace Broker.Application.Handler.Command
{
    public class SearchIntegration : BaseCriteria, IRequest<BaseSearchResponse<IntegrationDto>>
    {
        // public bool ClientOverride { get; set; } = false;
        public SearchIntegration()
        {
            Sorts = DefaultSearchConstants.DEFAULT_SORT;
        }
    }
}
