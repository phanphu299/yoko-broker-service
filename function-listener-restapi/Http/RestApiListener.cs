using HashidsNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Extension;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using System.Net.Http;
using AHI.Broker.Function.Model;
using Newtonsoft.Json;
using AHI.Infrastructure.MultiTenancy.Extension;

namespace AHI.Broker.Function.Trigger.Http
{
    public class RestApiListener
    {
        private readonly ITenantContext _tenantContext;
        private readonly IDataIngestionService _ingestionService;
        private readonly ICache _cache;
        private readonly ILoggerAdapter<RestApiListener> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public RestApiListener(ITenantContext tenantContext, IDataIngestionService ingestionService, ICache cache, ILoggerAdapter<RestApiListener> logger, IHttpClientFactory httpClientFactory)
        {
            _tenantContext = tenantContext;
            _ingestionService = ingestionService;
            _cache = cache;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("RestApiListener")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "fnc/bkr/api")] HttpRequest request, ILogger logger
        )
        {
            var validation = await ValidateApiKeyAsync(request.Query);
            if (!validation.Result)
                return new BadRequestResult();
            if (string.IsNullOrEmpty(validation.BrokerId))
            {
                return new BadRequestResult();
            }
            var brokerId = Guid.Parse(validation.BrokerId);
            var (contentType, fileName, contentStream) = await GetContentAsync(request);
            if (contentStream != null)
            {
                try
                {
                    await _ingestionService.IngestDataAsync(brokerId, contentType, contentStream, fileName);
                    return new OkResult();
                }
                catch (System.Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
            return new BadRequestResult();
        }

        private async Task<(bool Result, string BrokerId)> ValidateApiKeyAsync(IQueryCollection query)
        {
            if (!query.TryGetValue("id", out var values))
                return (false, null);

            var apiKey = values.FirstOrDefault();
            if (!ParseApiKey(apiKey, out var decodedIds))
                return (false, null);

            _tenantContext.SetTenantId(decodedIds[0].ToLowerInvariant());
            _tenantContext.SetSubscriptionId(decodedIds[1].ToLowerInvariant());
            _tenantContext.SetProjectId(decodedIds[2].ToLowerInvariant());

            if (!await IsValidApiKeyAsync(decodedIds[3], apiKey))
                return (false, null);

            var brokerId = decodedIds[3];
            return (true, brokerId);
        }

        private bool ParseApiKey(string apiKey, out string[] decodedIds)
        {
            var hasher = new Hashids();

            decodedIds = hasher.DecodeGuid(apiKey);
            if (decodedIds.Length != 4)
            {
                decodedIds = null;
                return false;
            }

            return true;
        }

        private async Task<bool> IsValidApiKeyAsync(string brokerId, string apiKey)
        {
            var key = $"{_tenantContext.TenantId}_{_tenantContext.SubscriptionId}_{_tenantContext.ProjectId}_broker_restapi_{brokerId}_apikey".ToLowerInvariant();
            var brokerKey = await _cache.GetStringAsync(key);
            if (brokerKey == null)
            {
                // need to get from broker-service
                var brokerService = _httpClientFactory.CreateClient(HttpClientNames.BROKER_SERVICE, _tenantContext);
                var brokerStream = await brokerService.GetByteArrayAsync($"bkr/brokers/{brokerId}");
                var brokerDto = brokerStream.Deserialize<BrokerDto>();
                if (brokerDto.Status == "AC")
                {
                    var brokerContent = JsonConvert.DeserializeObject<BrokerContentDto>(brokerDto.Content);
                    var endpointUri = new Uri(brokerContent.Endpoint);
                    var queryString = endpointUri.ParseQueryString();
                    brokerKey = queryString["id"];
                    await _cache.StoreAsync(key, brokerKey);
                }
            }
            return string.Equals(brokerKey, apiKey, StringComparison.InvariantCulture); // case sensitive
        }

        private async Task<(string, string, byte[])> GetContentAsync(HttpRequest request)
        {
            string contentType = null;
            string fileName = string.Empty;
            byte[] data = null;
            if (request.HasJsonContentType())
            {
                contentType = MimeType.JSON;
                data = new byte[request.Body.Length];
                await request.Body.ReadAsync(data, 0, data.Length);
            }
            else if (request.HasFormContentType)
            {
                var file = request.Form?.Files?.FirstOrDefault(x => x.ContentType == MimeType.CSV);
                if (file != null)
                {
                    fileName = file.FileName;
                    contentType = MimeType.CSV;
                    data = new byte[file.Length];
                    using (var stream = file.OpenReadStream())
                    {
                        await stream.ReadAsync(data, 0, data.Length);
                    }
                }
            }
            return (contentType, fileName, data);
        }
    }
}
