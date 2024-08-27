using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Function.Constant;
using Newtonsoft.Json;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace AHI.Broker.Function.Service
{
    public class AzureIoTDeviceRegistrationProvider : BaseCloudDeviceRegistrationProvider
    {
        public AzureIoTDeviceRegistrationProvider(ICloudDeviceRegistrationProvider next,
                                                  IHttpClientFactory httpClientFactory,
                                                  ITenantContext tenantContext)
        : base(next, httpClientFactory, tenantContext)
        {
        }

        protected override bool CanRegister(BrokerDto broker)
        {
            return string.Equals(broker.Type, "BROKER_IOT_HUB", System.StringComparison.InvariantCultureIgnoreCase);
        }

        protected override async Task<BrokerDto> RegisterAsync(BrokerDto broker, string deviceId)
        {
            // https://docs.microsoft.com/en-us/rest/api/iothub/service/devices/create-or-update-identity
            broker.Details = JsonConvert.DeserializeObject<IDictionary<string, object>>(broker.Content);
            var iotHubName = broker.Details["iot_hub_name"]?.ToString();
            if (string.IsNullOrEmpty(iotHubName))
            {
                throw new System.Exception("IoT Hub name is empty");
            }
            var resourceEndpoint = $"https://{iotHubName}.azure-devices.net/devices/{Uri.EscapeDataString(deviceId)}";
            var endpoint = $"{resourceEndpoint}?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";
            var tier = broker.Details["tier"];
            var iotEdge = false;
            var basicTiers = new[] { "S1", "S2", "S3" };
            if (basicTiers.Contains(tier))
            {
                iotEdge = true;
            }
            var requestBody = new
            {
                deviceId = deviceId,
                authentication = new
                {
                    symmetricKey = new
                    {
                        primaryKey = (object)null, //auto generate
                        secondaryKey = (object)null // auto generate
                    },
                    type = "sas"
                },
                capabilities = new
                {
                    iotEdge = iotEdge
                },
                status = "enabled"
            };
            var azureClient = _httpClientFactory.CreateClient("iothub-service");
            var token = await GetIoTManagementTokenAsync(broker);
            var sasToken = GenerateSASToken(resourceEndpoint, token);
            azureClient.DefaultRequestHeaders.Clear();
            azureClient.DefaultRequestHeaders.Add("Authorization", sasToken);
            var response = await azureClient.GetAsync(endpoint);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                response = await azureClient.PutAsync(endpoint, new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var payload = await response.Content.ReadAsByteArrayAsync();
                var iotInformation = payload.Deserialize<IoTDeviceResponse>();
                var deviceService = _httpClientFactory.CreateClient(HttpClientNames.DEVICE, _tenantContext);
                var deviceResponse = await deviceService.PatchAsync($"dev/devices/{deviceId}", new StringContent(JsonConvert.SerializeObject(new List<object> {
                        new {
                            op ="update/key",
                            path = $"device/{deviceId}",
                            value = iotInformation.Authentication
                        }
                 }), Encoding.UTF8, "application/json"));
                deviceResponse.EnsureSuccessStatusCode();
            }
            return broker;
        }

        protected override async Task<BrokerDto> UnRegisterAsync(BrokerDto broker, string deviceId)
        {
            // https://docs.microsoft.com/en-us/rest/api/iothub/service/devices/create-or-update-identity
            broker.Details = JsonConvert.DeserializeObject<IDictionary<string, object>>(broker.Content);
            var iotHubName = broker.Details["iot_hub_name"]?.ToString();
            if (string.IsNullOrEmpty(iotHubName))
            {
                throw new System.Exception("IoT Hub name is empty");
            }
            var resourceEndpoint = $"https://{iotHubName}.azure-devices.net/devices";
            var endpoint = $"{resourceEndpoint}?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";
            var azureClient = _httpClientFactory.CreateClient("iothub-service");
            var token = await GetIoTManagementTokenAsync(broker);
            var sasToken = GenerateSASToken(resourceEndpoint, token);
            azureClient.DefaultRequestHeaders.Clear();
            azureClient.DefaultRequestHeaders.Add("Authorization", sasToken);
            var body = new StringContent(JsonConvert.SerializeObject(new List<object>()
            {
                new {
                    etag = "*",
                    id = deviceId,
                    importMode = "delete"
                }
            }), System.Text.Encoding.UTF8, "application/json");
            await azureClient.PostAsync(endpoint, body);
            return broker;
        }
        private async Task<string> GetIoTManagementTokenAsync(BrokerDto broker)
        {
            var brokerContent = JsonConvert.DeserializeObject<IDictionary<string, object>>(broker.Content);
            var iotHubId = brokerContent["iot_hub_id"];
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var message = await azureClient.PostAsync($"{iotHubId}/IotHubKeys/iothubowner/listKeys?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
            message.EnsureSuccessStatusCode();
            var payload = await message.Content.ReadAsByteArrayAsync();
            var key = payload.Deserialize<SharedAccessSignatureAuthorizationRule>();
            return key.PrimaryKey;
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
    internal class IoTDeviceResponse
    {
        public IoTAuthentication Authentication { get; set; }
        public string DeviceId { get; set; }
    }
    internal class IoTAuthentication
    {
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }
}
