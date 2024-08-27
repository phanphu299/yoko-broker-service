using System.Threading;
using System.Threading.Tasks;
using AHI.Function.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface ILookupService
    {
        Task UpsertAsync(LookupInfo info);
        Task<LookupDto> ProcessLookUpFromConfigurationServiceAsync(string code, CancellationToken token = default);
    }
}
