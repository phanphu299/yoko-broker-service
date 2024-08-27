using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Infrastructure.Repository.Generic;

namespace Broker.Application.Repository.Abstraction
{
    public interface IBrokerRepository : IRepository<Domain.Entity.Broker, Guid>
    {
        Task<Domain.Entity.Broker> AddAsync(Domain.Entity.Broker e, IDictionary<string, object> details);
        Task<bool> RemoveBrokersAsync(IEnumerable<Guid> ids);
    }
}
