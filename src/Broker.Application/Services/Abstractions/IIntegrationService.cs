using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Device.Domain.Entity;
using AHI.Infrastructure.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;

namespace Broker.Application.Service.Abstraction
{
    public interface IIntegrationService : ISearchService<Domain.Entity.Integration, Guid, SearchIntegration, IntegrationDto>, IFetchService<Domain.Entity.Integration, Guid, IntegrationDto>
    {
        Task<IntegrationDto> FindByIdAsync(GetIntegrationById command, CancellationToken token);
        Task<BaseSearchResponse<FetchDataDto>> FetchDataAsync(FetchIntegrationData command, CancellationToken token);
        Task<IntegrationDto> AddAsync(AddIntegration command, CancellationToken token);
        Task<IEnumerable<TimeSeriesDto>> QueryTimeSeriesDataAsync(QueryIntegrationData command, CancellationToken cancellationToken);
        Task<IntegrationDto> UpdateAsync(UpdateIntegration command, CancellationToken token);
        Task<BaseResponse> RemoveAsync(DeleteIntegration command, CancellationToken token);
        Task<BaseResponse> CheckExistIntegrationsAsync(CheckExistIntegration command, CancellationToken token);
        Task<IEnumerable<ArchiveIntegrationDto>> ArchiveAsync(ArchiveIntegration command, CancellationToken token);
        Task<IDictionary<string, object>> RetrieveAsync(RetrieveIntegration command, CancellationToken token);
        Task<BaseResponse> VerifyArchiveDataAsync(VerifyIntegration command, CancellationToken token);
    }
}
