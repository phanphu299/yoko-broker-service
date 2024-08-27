using System.Net.Http;
using System.Threading.Tasks;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Function.Model;
using AHI.Infrastructure.SharedKernel.Extension;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using Function.Constant;
using Function.Model;
using System.Text;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Broker.Function.Constant;
using System;
using System.Web;
using System.Security.Cryptography;
using System.Globalization;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Extension;
using System.Linq;
using AHI.Infrastructure.Audit.Service.Abstraction;

namespace AHI.Broker.Function.Service
{
    public class AzureEventHubCloudProvider : BaseCloudProvider
    {
        private readonly ILoggerAdapter<AzureEventHubCloudProvider> _logger;
        private readonly AzureConfiguration _azureConfiguration;
        public AzureEventHubCloudProvider(ICloudProvider next,
                                        ITenantContext tenantContext,
                                        IHttpClientFactory httpClientFactory,
                                        ILoggerAdapter<AzureEventHubCloudProvider> logger,
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
            return string.Equals(broker.Type, "BROKER_EVENT_HUB", System.StringComparison.InvariantCultureIgnoreCase);
        }

        protected override bool CanRemove(BrokerDto broker)
        {
            return string.Equals(broker.Type, "BROKER_EVENT_HUB", System.StringComparison.InvariantCultureIgnoreCase);
        }

        protected override async Task<BrokerDto> CreateAsync(BrokerDto broker)
        {
            var eventHub = await CreateEventHubAsync(broker);
            var authorizationRuleSend = await CreateAuthorizationRuleAsync(eventHub.Id, "Send", new[] { "Send" });
            await CreateAuthorizationRuleAsync(eventHub.Id, "Listen", new[] { "Listen" });
            var brokerContent = JsonConvert.DeserializeObject<IDictionary<string, object>>(broker.Content);
            brokerContent["connection_string"] = authorizationRuleSend.PrimaryConnectionString;
            brokerContent["event_hub_id"] = eventHub.Id;
            broker.Details = brokerContent;
            string resourceUri = $"{authorizationRuleSend.PrimaryConnectionString.Split(';')[0].Split("//")[1]}{brokerContent["event_hub_name"]}";
            var sasToken = GenerateSasToken(resourceUri, "Send", authorizationRuleSend.PrimaryKey, Int32.Parse(broker.Details["sasTokenDuration"].ToString()));
            broker.Details["sasToken"] = sasToken;
            return broker;
        }

        protected override async Task RemoveAsync(BrokerDto broker)
        {
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var tenants = await _masterService.GetAllTenantsAsync();
            var tenant = tenants.Single(x => x.ResourceId == _tenantContext.TenantId);
            var eventhubNamespace = $"ahs-{tenant.LocationId}-{broker.Id.ToString().ToLowerInvariant().Replace("-", "")}-evh";
            var namespaceUri = $"subscriptions/{_azureConfiguration.SubscriptionId}/resourceGroups/{_azureConfiguration.ResourceGroup}/providers/Microsoft.EventHub/namespaces/{eventhubNamespace}";
            // check the event hub namespace exists or not.
            // if the namespace is not found -> create new one
            var response = await azureClient.GetAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                response = await azureClient.DeleteAsync(
                        $"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION_EVENT_DELETED}");
                response.EnsureSuccessStatusCode();
            }
        }

        private async Task<AzureKeyResponse> CreateAuthorizationRuleAsync(string hubId, string ruleName, string[] rights)
        {
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var ruleUri = $"{hubId}/authorizationRules/{ruleName}?api-version={AzureContants.AZURE_API_VERSION}";
            var response = await azureClient.GetAsync(ruleUri);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                response = await azureClient.PutAsync(ruleUri, new StringContent(JsonConvert.SerializeObject(new
                {
                    properties = new
                    {
                        rights = rights
                    }
                }), System.Text.Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
            }
            var message = await azureClient.PostAsync($"{hubId}/authorizationRules/{ruleName}/listKeys?api-version={AzureContants.AZURE_API_VERSION}", new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
            message.EnsureSuccessStatusCode();
            var payload = await message.Content.ReadAsByteArrayAsync();
            return payload.Deserialize<AzureKeyResponse>();
        }
        private async Task<AzureEventHubResponse> CreateEventHubAsync(BrokerDto broker)
        {
            var azureClient = _httpClientFactory.CreateClient(HttpClientNames.AZURE);
            var tenantService = _httpClientFactory.CreateClient(HttpClientNames.TENANT, _tenantContext);
            var tenants = await _masterService.GetAllTenantsAsync();
            var tenant = tenants.Single(x => x.ResourceId == _tenantContext.TenantId);
            var brokerDetail = JsonConvert.DeserializeObject<BrokerDetail>(broker.Content);
            var eventhubNamespace = $"ahs-{tenant.LocationId}-{broker.Id.ToString().ToLowerInvariant().Replace("-", "")}-evh";
            var namespaceUri = $"subscriptions/{_azureConfiguration.SubscriptionId}/resourceGroups/{_azureConfiguration.ResourceGroup}/providers/Microsoft.EventHub/namespaces/{eventhubNamespace}";
            // need to wait until the event namespace is active
            var waitTime = new CancellationTokenSource(15 * 60 * 1000);
            // check the event hub namespace exists or not.
            // if the namespace is not found -> create new one
            var response = await azureClient.GetAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION}");
            var projects = await _masterService.GetAllProjectsAsync();
            var project = projects.Single(x => x.ResourceId == _tenantContext.ProjectId);
            var hubTags = TagBuilder.BuidTags(_tenantContext, project.TenantName, project.SubscriptionName, project.Name, _azureConfiguration.Environment, namespaceUri);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var body = JsonConvert.SerializeObject(new
                {
                    type = "Microsoft.EventHub/namespaces",
                    location = tenant.LocationName,
                    properties = new
                    {
                        //Cannot set AutoInflate in Badic, Premium SKU in Eventhub. For more information visit https://aka.ms/eventhubsarmexceptions.
                        isAutoInflateEnabled = string.Equals(brokerDetail.Tier, "Standard", System.StringComparison.InvariantCultureIgnoreCase) ? brokerDetail.AutoInflate : false,
                        maximumThroughputUnits = string.Equals(brokerDetail.Tier, "Standard", System.StringComparison.InvariantCultureIgnoreCase) ? brokerDetail.MaxThroughputUnit : 0
                    },
                    tags = hubTags.ToDynamicObject(),
                    sku = new
                    {
                        name = brokerDetail.Tier
                    },
                    identity = new
                    {
                        type = "SystemAssigned"
                    }
                });
                _logger.LogInformation(body);
                response = await azureClient.PutAsync($"{namespaceUri}?api-version={AzureContants.AZURE_API_VERSION}", new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                var namespacePayload = await response.Content.ReadAsByteArrayAsync();
                var namespaceResponse = namespacePayload.Deserialize<AzureEventHubNamespaceResponse>();
                while (!waitTime.IsCancellationRequested)
                {
                    var nsResponse = await azureClient.GetAsync($"{namespaceResponse.Id}?api-version={AzureContants.AZURE_API_VERSION}");
                    var data = await nsResponse.Content.ReadAsByteArrayAsync();
                    var nsResult = data.Deserialize<AzureEventHubNamespaceResponse>();
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
            }
            var eventHubUri = $"{namespaceUri}/eventhubs/{brokerDetail.EventHubName}?api-version={AzureContants.AZURE_API_VERSION}";
            var eventHubContent = new StringContent(JsonConvert.SerializeObject(new
            {
                properties = new
                {
                    messageRetentionInDays = string.Equals(brokerDetail.Tier, "Basic", System.StringComparison.InvariantCultureIgnoreCase) ? 1 : 7,
                }
            }), System.Text.Encoding.UTF8, "application/json");
            while (!waitTime.IsCancellationRequested)
            {
                response = await azureClient.GetAsync(eventHubUri);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    response = await azureClient.PutAsync(eventHubUri, eventHubContent);
                }
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation(content);
                    break;
                }
                await Task.Delay(15 * 1000);
            }
            response = await azureClient.GetAsync(eventHubUri);
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadAsByteArrayAsync();
            return payload.Deserialize<AzureEventHubResponse>();
        }

        private string GenerateSasToken(string resourceUri, string keyName, string key, int duration)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var timeExpired = TimeSpan.FromDays(duration);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + timeExpired.TotalSeconds);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }
    }


    internal class AzureEventHubResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    internal class AzureEventHubNamespaceResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public AzureEventHubNamespaceProperty Properties { get; set; }
    }
    internal class AzureEventHubNamespaceProperty
    {
        public string Status { get; set; }
        public bool IsActivated => string.Equals(Status, "Active", System.StringComparison.InvariantCultureIgnoreCase);
    }
}
