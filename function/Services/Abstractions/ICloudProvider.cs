using System;
using System.Threading.Tasks;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface ICloudProvider
    {
        Task DeployAsync(Guid brokerId);
        Task RemoveAsync(Guid brokerId);
    }
}
