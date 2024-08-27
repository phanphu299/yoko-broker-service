using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Infrastructure.Repository.Generic;

namespace Broker.Application.Repository.Abstraction
{
    public interface IIntegrationRepository : IRepository<Domain.Entity.Integration, Guid>
    {
        Task<bool> RemoveListEntityWithRelationAsync(ICollection<Domain.Entity.Integration> entity);
    }
}
