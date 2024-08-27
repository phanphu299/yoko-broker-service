using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Broker.Function.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IBrokerService
    {
        Task<bool> CheckMqttAclAsync(CheckMqttAclRequest request);
        Task<bool> CheckEmqxAuthenticationAsync(CheckEmqxAuthenticationRequest request);
        Task AssignClientAsync(AssignClientRequest request);
        Task RemoveClientAsync(RemoveDeviceRequest request);
        Task RemoveEmqxBrokersAsync(RemoveEmqxBrokersRequest request);
        Task<IEnumerable<BrokerTopicDto>> GetBrokerTopicsAsync(GetBrokerTopicsRequest request);
    }
}
