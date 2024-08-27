using System.Threading;
using System.Threading.Tasks;
namespace SimulateEMQXDevice;

public interface IEmqxPublisher
{
    Task RunAsync(CancellationToken cancellationToken);
    Task StopAsync();
}
