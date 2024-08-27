using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Model;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Models;

namespace Broker.Application.Service.Abstraction
{
    public interface IBrokerService : ISearchService<Domain.Entity.Broker, Guid, SearchBroker, BrokerDto>, IFetchService<Domain.Entity.Broker, Guid, BrokerDto>
    {
        Task<BrokerDto> FindByIdAsync(GetBrokerById command, CancellationToken token);
        Task<BrokerDto> AddBrokerAsync(AddBroker command, CancellationToken token);
        Task<BrokerDto> UpdateBrokerAsync(UpdateBroker command, CancellationToken token);
        Task<BaseResponse> DeleteBrokerAsync(DeleteBrokerById command, CancellationToken token);
        Task<BaseResponse> DeleteBrokersAsync(DeleteBroker command, CancellationToken token);
        Task<ActivityResponse> ExportAsync(ExportBroker request, CancellationToken cancellationToken);
        Task<BaseResponse> CheckExistBrokersAsync(CheckExistBroker brokers, CancellationToken cancellationToken);
        Task<IEnumerable<ArchiveBrokerDto>> ArchiveAsync(ArchiveBroker command, CancellationToken token);
        Task<IDictionary<string, object>> RetrieveAsync(RetrieveBroker command, CancellationToken token);
        Task<BaseResponse> VerifyArchiveDataAsync(VerifyBroker command, CancellationToken token);
    }
}
