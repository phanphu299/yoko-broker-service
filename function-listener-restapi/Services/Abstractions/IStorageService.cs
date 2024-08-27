using System.Threading.Tasks;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface IStorageService
    {
        Task<string> UploadAsync(string path, string fileName, byte[] data);
    }
}