using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Constant;
using System.Security.Cryptography;
using System;
using System.Web;
using System.Globalization;
using System.Text;
using AHI.Broker.Function.Service;
using System.Linq;
using Function.Constant;
using AHI.Infrastructure.SharedKernel.Extension;

namespace AHI.Broker.Function.DelegatingHandler
{
    public class HttpMessageIotHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpMessageIotHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<string> GetTokenIot(string iotHubId, string resourceEndpoint)
        {
            var token = await GetIoTManagementTokenAsync(iotHubId);
            var sasToken = GenerateSASToken(resourceEndpoint, token);
            return sasToken;
        }
        private async Task<string> GetIoTManagementTokenAsync(string iotHubId)
        {
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var message = await azureClient.PostAsync($"{iotHubId}/listkeys?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
            var payload = await message.Content.ReadAsByteArrayAsync();
            var listKey = payload.Deserialize<ResponseListKey>();
            var keyIotHubOwner = listKey.Value.FirstOrDefault(x => x.KeyName == "iothubowner");
            return keyIotHubOwner?.PrimaryKey;
        }
        private string GenerateSASToken(string resourceUri, string key, string keyName = "iothubowner")
        {
            resourceUri = resourceUri.TrimStart("https://".ToCharArray());
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var five_minutes = TimeSpan.FromMinutes(5);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + five_minutes.TotalSeconds);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }
}