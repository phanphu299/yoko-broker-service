using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;

namespace Broker.Application.Service.Abstraction
{
    public interface ISchemaService
    {
        Task<IntegrationSchemaDto> FindByTypeAsync(GetSchemaByType command, CancellationToken token);
    }
}
