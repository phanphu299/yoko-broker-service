using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IDeviceService
    {
        Task<BaseResponse> ValidateIngestionAsync(string filePath);
    }
}