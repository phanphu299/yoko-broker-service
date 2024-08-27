using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;

namespace Broker.Application.Service.Abstraction
{
    public interface IEmqxService
    {
        Task<IEnumerable<MqttDto>> GetAllMqttAsync(GetValidMqtt request, CancellationToken cancellationToken);
        Task<IEnumerable<CoapDto>> GetAllCoapAsync(GetValidCoap request, CancellationToken cancellationToken);
    }
}