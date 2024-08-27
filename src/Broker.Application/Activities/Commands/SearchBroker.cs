using AHI.Infrastructure.SharedKernel.Model;
using MediatR;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Constants;

namespace Broker.Application.Handler.Command
{
    public class SearchBroker : BaseCriteria, IRequest<BaseSearchResponse<BrokerDto>>
    {
        // public bool ClientOverride { get; set; } = false;
        public SearchBroker()
        {
            Sorts = DefaultSearchConstants.DEFAULT_SORT;
        }
    }
}
