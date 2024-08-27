using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IEntityTagService
    {
        Task<IEnumerable<Guid>> GetEntityIdsByTagIdsAsync(long[] tagIds);
        Task RemoveBrokerDetailCacheAsync(IEnumerable<Guid> brokerIds);
    }
}