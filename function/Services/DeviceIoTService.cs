using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.DelegatingHandler;
using AHI.Broker.Function.Extension;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Exception.Helper;
using AHI.Infrastructure.SharedKernel.Extension;
using CoAP;
using Function.Constant;
using Function.Helper;
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AHI.Broker.Function.Service
{
    public class DeviceIotService : IDeviceIotService
    {
        private readonly HttpMessageIotHandler _httpMessageIotHandler;
        private readonly IHttpClientFactory _httpClientFactory;
        public DeviceIotService(IHttpClientFactory httpClientFactory,
                                HttpMessageIotHandler httpMessageIotHandler)
        {
            _httpClientFactory = httpClientFactory;
            _httpMessageIotHandler = httpMessageIotHandler;
        }
        public async Task<DeviceResponse> RegisterDeviceIotAsync(DeviceIotRequestMessage deviceIot)
        {
            var brokerContent = ParseBrokerContent(deviceIot.BrokerContent);

            if (string.IsNullOrEmpty(brokerContent.iotHubName))
            {
                throw new Exception("IoT Hub name is empty");
            }

            if (!Regex.IsMatch(deviceIot.DeviceId, RegexConstants.IOT_HUB_DEVICE_ID))
            {
                throw ValidationExceptionHelper.GenerateInvalidValidation(nameof(deviceIot.DeviceId), MessageConstants.DEVICE_ID_UNMATCHED_BROKER_TYPE);
            }

            var resourceEndpoint = $"https://{brokerContent.iotHubName}.azure-devices.net/devices/{Uri.EscapeDataString(deviceIot.DeviceId)}";
            var endpoint = $"{resourceEndpoint}?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";

            var requestBody = GetRequestBodyByAuthenticationType(deviceIot);
            var azureClient = _httpClientFactory.CreateClient("iothub-service");

            var tokenSas = await _httpMessageIotHandler.GetTokenIot(brokerContent.iotHubId, resourceEndpoint);
            azureClient.AddAuthenticationHeader(tokenSas);
            var response = await CheckExistingDeviceAsync(deviceIot.DeviceId, brokerContent.iotHubName, brokerContent.iotHubId);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                response = await azureClient.PutAsync(endpoint, new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var data = await response.Content.ReadAsByteArrayAsync();
                var iotInformation = data.Deserialize<DeviceResponse>();
                return iotInformation;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // the device already registered. -> need to throw the exception
                throw ValidationExceptionHelper.GenerateDuplicateValidation(nameof(deviceIot.DeviceId));
            }
            return null;
        }

        public (string connectionString, string iotHubName, string iotHubId) ParseBrokerContent(string brokerContent)
        {
            var contentJsonParse = JObject.Parse(brokerContent);
            return (contentJsonParse["connection_string"].ToString(), contentJsonParse["iot_hub_name"].ToString(), contentJsonParse["iot_hub_id"].ToString());
        }

        public async Task<HttpResponseMessage> CheckExistingDeviceAsync(string deviceId, string iotHubName, string iotHubId)
        {
            var resourceEndpoint = $"https://{iotHubName}.azure-devices.net/devices/{Uri.EscapeDataString(deviceId)}";
            var endpoint = $"{resourceEndpoint}?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";
            var azureClient = _httpClientFactory.CreateClient("iothub-service");

            var tokenSas = await _httpMessageIotHandler.GetTokenIot(iotHubId, resourceEndpoint);
            azureClient.AddAuthenticationHeader(tokenSas);
            return await azureClient.GetAsync(endpoint);
        }

        private string GetRequestBodyByAuthenticationType(DeviceIotRequestMessage deviceIot)
        {
            var contentJsonParse = JObject.Parse(deviceIot.BrokerContent);
            var tier = contentJsonParse["tier"].ToString();
            var iotEdge = false;
            var basicTiers = new[] { "S1", "S2", "S3" };
            if (basicTiers.Contains(tier))
            {
                iotEdge = true;
            }
            var requestBody = new RequestAuthenticationType();
            requestBody.DeviceId = deviceIot.DeviceId;
            requestBody.Status = RequestDeviceIotStatusConstants.ENABLED;
            requestBody.Capabilities = new { iotEdge = iotEdge };
            if (deviceIot.IoTAuthenticationType == DeviceAuthenticationType.X509_SELF_SIGNED)
            {
                requestBody.Authentication = new
                {
                    x509Thumbprint = new
                    {
                        primaryThumbprint = deviceIot.PrimaryThumbprint,
                        secondaryThumbprint = deviceIot.SecondaryThumbprint
                    },
                    type = DeviceAuthenticationType.X509_SELF_SIGNED
                };
            }
            else if (deviceIot.IoTAuthenticationType == DeviceAuthenticationType.X509_CA_SIGNED)
            {
                requestBody.Authentication = new
                {
                    type = DeviceAuthenticationType.X509_CA_SIGNED
                };
            }
            else
            {
                requestBody.Authentication = new
                {
                    symmetricKey = new
                    {
                        primaryKey = (object)null, //auto generate
                        secondaryKey = (object)null // auto generate
                    },
                    type = DeviceAuthenticationType.SYMMETRIC_KEY
                };
            }
            return JsonConvert.SerializeObject(requestBody);
        }
        public async Task RegeneratePrimaryKeyAsync(DeviceIotRequestMessage deviceIot)
        {
            var brokerContent = ParseBrokerContent(deviceIot.BrokerContent);
            if (string.IsNullOrEmpty(brokerContent.iotHubName))
            {
                throw new System.Exception("IoT Hub name is empty");
            }
            var resourceEndpoint = $"https://{brokerContent.iotHubName}.azure-devices.net/devices/{Uri.EscapeDataString(deviceIot.DeviceId)}";
            var endpoint = $"{resourceEndpoint}?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";

            var requestBody = new
            {
                deviceId = deviceIot.DeviceId,
                authentication = new
                {
                    symmetricKey = new
                    {
                        primaryKey = deviceIot.PrimaryKey,
                        secondaryKey = deviceIot.SecondaryKey
                    }
                }
            };

            var azureClient = _httpClientFactory.CreateClient("iothub-service");
            var tokenSas = await _httpMessageIotHandler.GetTokenIot(brokerContent.iotHubId, resourceEndpoint);
            azureClient.AddAuthenticationDeviceChangedHeader(tokenSas);

            var response = await azureClient.PutAsync(endpoint, new StringContent(JsonConvert.SerializeObject(requestBody), System.Text.Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }
        public async Task<bool> UnRegisterAsync(DeviceIotRequestMessage deviceIot)
        {
            var contentJsonParse = JObject.Parse(deviceIot.BrokerContent);
            if (contentJsonParse.ContainsKey("iot_hub_name") && contentJsonParse.ContainsKey("iot_hub_id"))
            {
                var brokerContent = ParseBrokerContent(deviceIot.BrokerContent);

                if (string.IsNullOrEmpty(brokerContent.iotHubName))
                {
                    throw new Exception("IoT Hub name is empty");
                }
                var resourceEndpoint = $"https://{brokerContent.iotHubName}.azure-devices.net/devices";
                var endpoint = $"{resourceEndpoint}?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";
                var azureClient = _httpClientFactory.CreateClient("iothub-service");
                var tokenSas = await _httpMessageIotHandler.GetTokenIot(brokerContent.iotHubId, resourceEndpoint);
                azureClient.AddAuthenticationDeviceChangedHeader(tokenSas);
                var response = await azureClient.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(new[]{
                                                                                                                new {
                                                                                                                        id = deviceIot.DeviceId,
                                                                                                                        importMode = "delete",
                                                                                                                        etag="*"
                                                                                                                    }
                                                        }), System.Text.Encoding.UTF8, "application/json"));
                return response.IsSuccessStatusCode;
            }
            return true;
        }

        public async Task<HttpResponseMessage> PushMessageToDeviceIotAsync(DeviceIotRequestMessage deviceIot)
        {
            var contentJsonParse = JObject.Parse(deviceIot.BrokerContent);

            HttpResponseMessage response = new HttpResponseMessage();
            if (BrokerType.EMQX_BROKERS.Contains(deviceIot.BrokerType))
            {
                var deviceContentJsonParse = JObject.Parse(deviceIot.DeviceContent);

                if (deviceIot.BrokerType == BrokerType.EMQX_MQTT)
                {
                    response = await PushMessageToMqttDeviceAsync(contentJsonParse, deviceContentJsonParse, deviceIot.MessageContent);
                }
                else
                {
                    response = PushMessageToCoapDevice(contentJsonParse, deviceContentJsonParse, deviceIot.MessageContent);
                }
            }
            else
            {
                var brokerContent = ParseBrokerContent(deviceIot.BrokerContent);

                if (string.IsNullOrEmpty(brokerContent.iotHubName))
                {
                    throw new Exception("IoT Hub name is empty");
                }

                if (!BrokerType.IOT_HUB_STANDARD_TIER.Contains(contentJsonParse["tier"].ToString()))
                {
                    throw new Exception("Tier is not standard");
                }

                var resourceEndpoint = $"https://{brokerContent.iotHubName}.azure-devices.net/devices/{Uri.EscapeDataString(deviceIot.DeviceId)}";
                var endpoint = $"{resourceEndpoint}/messages/deviceBound?api-version={AzureContants.AZURE_API_VERSION_IOT_DEVICE}";
                var azureClient = _httpClientFactory.CreateClient("iothub-service");
                var tokenSas = await _httpMessageIotHandler.GetTokenIot(brokerContent.iotHubId, resourceEndpoint);
                azureClient.AddAuthenticationHeader(tokenSas);
                response = await azureClient.PostAsync(endpoint, new StringContent(deviceIot.MessageContent, System.Text.Encoding.UTF8, "text/plain"));
                response.EnsureSuccessStatusCode();
            }

            return response;
        }

        private async Task<HttpResponseMessage> PushMessageToMqttDeviceAsync(JObject brokerContent, JObject deviceContent, string payload)
        {
            MqttClientPublishResult result = null;
            var mqttFactory = new MqttFactory();
            using (var mqttClient = mqttFactory.CreateMqttClient())
            {
                var mqttClientOptions = new MqttClientOptionsBuilder()
                    .WithTcpServer(brokerContent[BrokerContentKeys.HOST]?.ToString())
                    .WithCredentials(deviceContent["username"]?.ToString(), deviceContent["password"]?.ToString())
                    .Build();

                await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
                string topic = brokerContent[BrokerContentKeys.COMMAND_TOPIC]?.ToString();
                var applicationMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build();

                result = await mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                await mqttClient.DisconnectAsync();
            }
            return result.ReasonCode == MqttClientPublishReasonCode.Success ?
                new HttpResponseMessage(System.Net.HttpStatusCode.OK) : new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        }
        private HttpResponseMessage PushMessageToCoapDevice(JObject brokerContent, JObject deviceContent, string payload)
        {
            var clientId = Guid.NewGuid();

            var host = brokerContent[BrokerContentKeys.HOST]?.ToString();
            if (string.IsNullOrEmpty(host))
            {
                var uriCommand = brokerContent[BrokerContentKeys.URI_COMMAND]?.ToString();

                if (string.IsNullOrEmpty(uriCommand))
                {
                    uriCommand = string.Empty;
                }
                else if (uriCommand.TrimEnd('/').EndsWith("$ahi/commands"))
                {
                    uriCommand = uriCommand.TrimEnd('/').TrimEnd("/$ahi/commands", StringComparison.InvariantCultureIgnoreCase);
                }
                host = uriCommand;
            }
            string topic = brokerContent[BrokerContentKeys.COMMAND_TOPIC]?.ToString();
            var uriWithTopic = $"{host}/{topic}";
            var coapUri = new Uri(uriWithTopic);
            string server = $"{coapUri.Host}{(coapUri.Port > 0 ? ":" + coapUri.Port : string.Empty)}";
            var client = new CoapClient(new Uri($"coap://{server}/mqtt/connection?clientid={clientId}&username={deviceContent["username"]?.ToString()}&password={deviceContent["password"]?.ToString()}"));
            var response = client.Post("");
            var token = response.ResponseText;

            var postClient = new CoapClient(new Uri($"{coapUri}?clientid={clientId}&token={token}&qos=1"));
            var clientResponse = postClient.Post(payload);

            client.Uri = new Uri($"coap://{server}/mqtt/connection?clientid={clientId}&token={token}");
            client.Delete();
            return clientResponse.StatusCode == StatusCode.Changed ?
                new HttpResponseMessage(System.Net.HttpStatusCode.OK) : new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
        }
    }
    internal class RequestAuthenticationType
    {
        public string DeviceId { get; set; }
        public object Authentication { get; set; }
        public object Capabilities { get; set; }
        public string Status { get; set; }
    }
    internal class ResponseListKey
    {
        public List<SharedAccessSignatureAuthorizationRule> Value { get; set; }
    }

    public class DeviceResponse
    {
        public string DeviceId { get; set; }
        public AuthenticationResponse Authentication { get; set; }
    }
    public class AuthenticationResponse
    {
        public SymmetricKey SymmetricKey { get; set; }
        public X509Thumbprint X509Thumbprint { get; set; }
    }
    public class SymmetricKey
    {
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }

    public class X509Thumbprint
    {
        public string PrimaryThumbprint { get; set; }
        public string SecondaryThumbprint { get; set; }
    }

    public class SharedAccessSignatureAuthorizationRule
    {
        public string KeyName { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }
}
