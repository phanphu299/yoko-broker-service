using System.Threading.Tasks;

namespace Broker.Application.Service.Abstractions
{
    public interface ISystemContext
    {
        Task<string> GetValueAsync(string key, string defaultValue, bool useCache = true);
    }
}