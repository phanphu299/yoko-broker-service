using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IDeviceIotService
    {
        Task<DeviceResponse> RegisterDeviceIotAsync(DeviceIotRequestMessage deviceIot);
        Task<bool> UnRegisterAsync(DeviceIotRequestMessage deviceIot);
        Task RegeneratePrimaryKeyAsync(DeviceIotRequestMessage deviceIot);
        Task<HttpResponseMessage> PushMessageToDeviceIotAsync(DeviceIotRequestMessage deviceIot);
        Task<HttpResponseMessage> CheckExistingDeviceAsync(string deviceId, string iotHubName, string iotHubId);
        (string connectionString, string iotHubName, string iotHubId) ParseBrokerContent(string brokerContent);
    }
}
