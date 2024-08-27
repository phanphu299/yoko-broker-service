using System.Threading;
using System.Threading.Tasks;

namespace Broker.Application.Service.Abstraction
{
    public interface ILookupService
    {
        Task<Domain.Entity.Lookup> ProcessLookUpFromConfigurationServiceAsync(string code, CancellationToken token);
    }
}
