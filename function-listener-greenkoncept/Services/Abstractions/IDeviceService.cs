using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Function.Service.Abstraction
{
    public interface IDeviceService
    {
        Task<IEnumerable<string>> GetDeviceAsync(Guid integrationId);
    }
}