using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Broker.Function.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IMasterService
    {
        Task<IEnumerable<MqttDto>> GetAllMqttBrokersAsync();
    }
}
