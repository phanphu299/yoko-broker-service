using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IExportHandler
    {
        Task<string> HandleAsync(ExecutionContext context, IEnumerable<string> ids);
    }
}
