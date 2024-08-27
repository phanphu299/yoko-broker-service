using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Broker.Function.Constant;
using AHI.Infrastructure.MultiTenancy.Extension;
using System.Text;

namespace AHI.Broker.Function.Service
{
    public class StorageService : IStorageService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITenantContext _tenantContext;

        public StorageService(IHttpClientFactory httpClientFactory, ITenantContext tenantContext)
        {
            _httpClientFactory = httpClientFactory;
            _tenantContext = tenantContext;
        }

        public async Task<string> UploadAsync(string path, string fileName, byte[] data)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.STORAGE, _tenantContext);

            var fileContent = new ByteArrayContent(data);
            var response = await UploadFileAsync(httpClient, path, fileName, fileContent);

            var responseContent = await response.Content.ReadAsByteArrayAsync();
            var filePath = responseContent.Deserialize<JObject>()["filePath"].ToString();

            return filePath;
        }

        private async Task<HttpResponseMessage> UploadFileAsync(HttpClient storageClient, string path, string fileName, HttpContent fileContent)
        {
            HttpResponseMessage response;
            var link = await GetLinkAsync(storageClient, path, skipCheckExists: true);
            path = new Uri(link).PathAndQuery.TrimStart('/'); // extract file path from returned url
            using (var content = new MultipartFormDataContent())
            {
                content.Add(fileContent, "file", fileName);

                response = await storageClient.PostAsync(path, content);
            }
            response.EnsureSuccessStatusCode();
            return response;
        }

        private async Task<string> GetLinkAsync(HttpClient storageClient, string path, bool skipCheckExists = false)
        {
            var requestBody = new { FilePath = path, SkipCheckExists = skipCheckExists }.ToJson();
            var response = await storageClient.PostAsync($"sta/files/link", new StringContent(requestBody, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
