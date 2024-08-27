using System;
using System.Threading;
using System.Threading.Tasks;

namespace Broker.Application.Service.Abstraction
{
    public interface IListenerService
    {
        Task<bool> ActiveAsync(Guid id, CancellationToken cancellationToken);
    }
}