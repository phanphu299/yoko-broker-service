using System.Net.Http;
using System.Threading.Tasks;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.SharedKernel.Extension;
using Newtonsoft.Json;
using System.Collections.Generic;
using Function.Constant;
using AHI.Infrastructure.SharedKernel.Abstraction;
using System.Text;
using System.Threading;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Service.Model;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Extension;
using System.Linq;
using AHI.Infrastructure.Audit.Service.Abstraction;

namespace AHI.Broker.Function.Service
{
    public class AzureIoTHubCloudProvider : BaseCloudProvider
    {
        private readonly ILoggerAdapter<AzureIoTHubCloudProvider> _logger;
        private readonly AzureConfiguration _azureConfiguration;
        public AzureIoTHubCloudProvider(ICloudProvider next,
                                        ITenantContext tenantContext,
                                        IHttpClientFactory httpClientFactory,
                                        ILoggerAdapter<AzureIoTHubCloudProvider> logger,
                                        INotificationService notificationService,
                                        IMasterService masterService,
                                        AzureConfiguration azureConfiguration
                                        ) : base(next, httpClientFactory, tenantContext, masterService, notificationService)
        {
            _logger = logger;
            _azureConfiguration = azureConfiguration;
        }

        protected override bool CanCreate(BrokerDto broker)
        {
            return string.Equals(broker.Type, "BROKER_IOT_HUB", System.StringComparison.InvariantCultureIgnoreCase);
        }

        protected override bool CanRemove(BrokerDto broker)
        {
            return string.Equals(broker.Type, "BROKER_IOT_HUB", System.StringComparison.InvariantCultureIgnoreCase);
        }

        protected override async Task<BrokerDto> CreateAsync(BrokerDto broker)
        {
            var iotHub = await CreateIoTHubAzureAsync(broker);
            var brokerContent = JsonConvert.DeserializeObject<IDictionary<string, object>>(broker.Content);
            var (connectionString, eventHub) = await GetListenConnectionStringAsync(iotHub);
            brokerContent["connection_string"] = connectionString;
            brokerContent["event_hub_name"] = eventHub;
            brokerContent["iot_hub_id"] = iotHub.Id;
            brokerContent["iot_hub_name"] = iotHub.Name;
            broker.Details = brokerContent;
            return broker;
        }

        private async Task<AzureIoTHubResponse> CreateIoTHubAzureAsync(BrokerDto broker)
        {
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var tenantService = _httpClientFactory.CreateClient(HttpClientNames.TENANT, _tenantContext);
            var brokerDetail = JsonConvert.DeserializeObject<BrokerIoTDetail>(broker.Content);
            var tenants = await _masterService.GetAllTenantsAsync();
            var tenant = tenants.Single(x => x.ResourceId == _tenantContext.TenantId);
            var iotHubName = $"ahs-{tenant.LocationId}-{broker.Id.ToString().ToLowerInvariant().Replace("-", "")}-iot";
            var namespaceUri = $"subscriptions/{_azureConfiguration.SubscriptionId}/resourceGroups/{_azureConfiguration.ResourceGroup}/providers/Microsoft.devices/IotHubs/{iotHubName}";
            // check the event hub namespace exists or not.
            // if the namespace is not found -> create new one
            var response = await azureClient.GetAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION_IOT}");
            var waitTime = new CancellationTokenSource(15 * 60 * 1000);
            var projects = await _masterService.GetAllProjectsAsync();
            var project = projects.Single(x => string.Equals(x.ResourceId, _tenantContext.ProjectId, System.StringComparison.InvariantCultureIgnoreCase));
            var hubTags = TagBuilder.BuidTags(_tenantContext, project.TenantName, project.SubscriptionName, project.Name, _azureConfiguration.Environment, namespaceUri);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                response = await azureClient.PutAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION_IOT}",
                    new StringContent(
                        JsonConvert.SerializeObject(new
                        {
                            name = iotHubName,
                            location = tenant.LocationName,
                            sku = new
                            {
                                name = brokerDetail.Tier,
                                capacity = 1
                            },
                            tags = hubTags.ToDynamicObject(),
                        }), System.Text.Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var namespacePayload = await response.Content.ReadAsByteArrayAsync();
                var namespaceResponse = namespacePayload.Deserialize<AzureIoTHubResponse>();
                while (!waitTime.IsCancellationRequested)
                {
                    var nsResponse = await azureClient.GetAsync($"{namespaceResponse.Id}?api-version={AzureContants.AZURE_API_VERSION_IOT}");
                    var data = await nsResponse.Content.ReadAsByteArrayAsync();
                    var nsResult = data.Deserialize<AzureIoTHubResponse>();
                    if (nsResult.Properties.IsActivated)
                    {
                        _logger.LogInformation(Encoding.UTF8.GetString(data));
                        break;
                    }
                    else
                    {
                        _logger.LogInformation($"Waiting the namespace to be created");
                        await Task.Delay(15 * 1000);
                    }

                }
                _logger.LogInformation($"Created successfully");
                return namespaceResponse;
            }
            else
            {
                var namespacePayload = await response.Content.ReadAsByteArrayAsync();
                var namespaceResponse = namespacePayload.Deserialize<AzureIoTHubResponse>();
                return namespaceResponse;
            }
        }

        protected override async Task RemoveAsync(BrokerDto broker)
        {
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var tenantService = _httpClientFactory.CreateClient(HttpClientNames.TENANT, _tenantContext);
            var tenants = await _masterService.GetAllTenantsAsync();
            var tenant = tenants.Single(x => x.ResourceId == _tenantContext.TenantId);
            var iotHubName = $"ahs-{tenant.LocationId}-{broker.Id.ToString().ToLowerInvariant().Replace("-", "")}-iot";
            var namespaceUri = $"subscriptions/{_azureConfiguration.SubscriptionId}/resourceGroups/{_azureConfiguration.ResourceGroup}/providers/Microsoft.devices/IotHubs/{iotHubName}";
            // check the event hub namespace exists or not.
            // if the namespace is not found -> create new one
            var response = await azureClient.GetAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION_IOT}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response = await azureClient.DeleteAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION_IOT}");
                response.EnsureSuccessStatusCode();
            }
        }
        // private async Task<AzureKeyResponse> GetDeviceConnectionStringAsync(AzureIoTHubResponse iotHub)
        // {
        //     var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
        //     var message = await azureClient.PostAsync($"{iotHub.Id}/IotHubKeys/device/listKeys?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
        //     message.EnsureSuccessStatusCode();
        //     var payload = await message.Content.ReadAsByteArrayAsync();
        //     var key = payload.Deserialize<SharedAccessSignatureAuthorizationRule>();
        //     var keyResponse = new AzureKeyResponse();
        //     keyResponse.PrimaryConnectionString = $"HostName={iotHub.Name}.azure-devices.net;SharedAccessKeyName=device;SharedAccessKey={key.PrimaryKey}";
        //     return keyResponse;
        // }
        private async Task<(string, string)> GetListenConnectionStringAsync(AzureIoTHubResponse iotHub)
        {
            // https://docs.microsoft.com/en-us/rest/api/iothub/iot-hub-resource/get-keys-for-key-name
            try
            {
                string keyName = "service";
                var azureClient = _httpClientFactory.CreateClient("azure-service");
                var message = await azureClient.PostAsync($"{iotHub.Id}/IotHubKeys/{keyName}/listKeys?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
                message.EnsureSuccessStatusCode();
                var payload = await message.Content.ReadAsByteArrayAsync();
                var key = payload.Deserialize<SharedAccessSignatureAuthorizationRule>();
                var primaryKey = key.PrimaryKey;
                var eventHubRequest = await azureClient.GetAsync($"{iotHub.Id}?api-version={AzureContants.AZURE_IOT_KEY_API_VERSION}");
                eventHubRequest.EnsureSuccessStatusCode();
                var eventHubBody = await eventHubRequest.Content.ReadAsByteArrayAsync();
                var eventHub = eventHubBody.Deserialize<IoTHubDto>();
                return ($"Endpoint={eventHub.Properties.EventHubEndpoints.Events.Endpoint};SharedAccessKeyName={keyName};SharedAccessKey={primaryKey};EntityPath={eventHub.Properties.EventHubEndpoints.Events.Path}",
                    eventHub.Properties.EventHubEndpoints.Events.Path);
            }
            catch (System.Exception exc)
            {
                _logger.LogError(exc, exc.Message);
            }
            return (null, null);
        }
    }


    internal class BrokerIoTDetail
    {
        public string Tier { get; set; } = "B1";
        [JsonProperty("number_of_hub_units")]
        public int NumberOfHubUnits { get; set; }
        [JsonProperty("device_to_cloud_partitions")]
        public int DeviceToCloudPartitions { get; set; }
        [JsonProperty("defender_for_iot")]
        public bool DefenderForIoT { get; set; }
    }

    internal class AzureIoTHubResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public AzureIoTHubProperty Properties { get; set; }
    }
    internal class AzureIoTHubProperty
    {
        public string State { get; set; }
        public bool IsActivated => string.Equals(State, "Active", System.StringComparison.InvariantCultureIgnoreCase);
    }
}
