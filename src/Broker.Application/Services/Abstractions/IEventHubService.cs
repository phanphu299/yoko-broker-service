using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;

namespace Broker.Application.Service.Abstraction
{
    public interface IEventHubService
    {
        Task<IEnumerable<EventHubDto>> GetAllEventHubAsync(GetValidEventHub request, CancellationToken cancellationToken);
    }
}