using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;

namespace Broker.Application.Service.Abstraction
{
    public interface IBrokerValidator
    {
        Task ValidateAsync(AddBroker broker, CancellationToken token);
        Task ValidateAsync(UpdateBroker broker, CancellationToken token);
    }
}