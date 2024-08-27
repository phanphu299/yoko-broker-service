using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Broker.Function.Service.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IMasterService
    {
        Task<IEnumerable<TenantDto>> GetAllTenantsAsync(bool migrated = true, bool deleted = false);
        Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(bool migrated = true, bool deleted = false);
    }
}
