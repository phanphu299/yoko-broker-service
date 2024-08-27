using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Repository.Generic;

namespace Broker.Application.Repository.Abstraction
{
    public interface ILookupRepository : IRepository<Domain.Entity.Lookup, string>
    {
        Task<Domain.Entity.Lookup> SaveAsync(Domain.Entity.Lookup entity);

        Task<Domain.Entity.Lookup> FindLookupByCodeAsync(string code, CancellationToken cancellationToken = default);
    }
}
