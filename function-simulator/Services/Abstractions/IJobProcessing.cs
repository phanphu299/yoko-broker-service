using System.Threading.Tasks;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IJobProcessing
    {
        Task ProcessAsync(string jobName);
    }
}