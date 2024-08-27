using System.Collections.Generic;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Domain.Entity;
using Device.Domain.Entity;
using AHI.Infrastructure.SharedKernel.Model;

namespace Broker.Application.Service.Abstraction
{
    public interface IIntegrationHandler
    {
        Task<BaseSearchResponse<FetchDataDto>> FetchAsync(Integration integration, FetchIntegrationData command);
        Task<IEnumerable<TimeSeriesDto>> QueryAsync(Integration integration, QueryIntegrationData command);
    }
}
