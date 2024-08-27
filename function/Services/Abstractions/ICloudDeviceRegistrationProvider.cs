using System;
using System.Threading.Tasks;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface ICloudDeviceRegistrationProvider
    {
        Task RegisterAsync(Guid brokerId, string projectId, string deviceId);
        Task UnRegisterAsync(Guid brokerId, string projectId, string deviceId);
    }
}
