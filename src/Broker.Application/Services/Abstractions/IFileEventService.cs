using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Broker.Application.Service.Abstraction
{
    public interface IFileEventService
    {
        Task SendImportEventAsync(string objectType, IEnumerable<string> data);
        Task SendExportEventAsync(Guid activityId, string objectType, IEnumerable<string> data);
    }
}
