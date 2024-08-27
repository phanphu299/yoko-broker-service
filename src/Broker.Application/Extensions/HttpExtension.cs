using System.Net.Http;
using System.Threading.Tasks;
using AHI.Infrastructure.SharedKernel.Extension;

namespace Broker.ApplicationExtension.Extension
{
    public static class HttpExtension
    {
        public static async Task<T> ReadJsonContentAsync<T>(this HttpResponseMessage response)
        {
            var responseContent = await response.Content.ReadAsByteArrayAsync();
            return responseContent.Deserialize<T>();
        }
    }
}