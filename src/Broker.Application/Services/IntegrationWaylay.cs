using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using Broker.Domain.Entity;
using Device.Domain.Entity;
using AHI.Infrastructure.SharedKernel.Model;
using Newtonsoft.Json;
using AHI.Infrastructure.SharedKernel.Extension;
using Broker.ApplicationExtension.Extension;
using Broker.Application.Constant;
using Configuration.Application.Constant;

namespace Broker.Application.Service
{
    public class IntegrationWaylay : IIntegrationHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static IDictionary<string, HttpClient> _cacheHttpClient = new Dictionary<string, HttpClient>();
        private readonly IDictionary<string, Func<string, Integration, Task<BaseSearchResponse<FetchDataDto>>>> _methods;

        public IntegrationWaylay(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _methods = new Dictionary<string, Func<string, Integration, Task<BaseSearchResponse<FetchDataDto>>>>()
            {
                { IntegrationTypeConstants.WAY_LAY_DEVICE_FETCH_TYPE, GetResourcesAsync },
                { IntegrationTypeConstants.WAY_LAY_METRIC_FETCH_TYPE, GetMetricAsync },
            };
        }
        public Task<BaseSearchResponse<FetchDataDto>> FetchAsync(Integration integration, FetchIntegrationData command)
        {
            var type = command.Type;
            var data = command.Data;

            if (_methods.ContainsKey(type))
            {
                return _methods[type].Invoke(data, integration);
            }
            else
            {
                throw new GenericProcessFailedException(MessageConstants.COMMON_ERROR_NO_HANDLER);
            }

        }
        private HttpClient CreateHttpClient(ApiInformation payload, string baseAddress)
        {
            if (_cacheHttpClient.ContainsKey(baseAddress))
            {
                return _cacheHttpClient[baseAddress];
            }
            if (!string.IsNullOrEmpty(payload.Endpoint))
            {
                var httpClient = _httpClientFactory.CreateClient(baseAddress);
                httpClient.BaseAddress = new Uri(baseAddress);
                string encodedToken = System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{payload.ApiKey}:{payload.ApiSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(payload.TokenType, encodedToken);
                _cacheHttpClient[baseAddress] = httpClient;
                return httpClient;

            }
            throw new GenericProcessFailedException("Can not create the httpclient because the endpoint is not defined");
        }
        protected virtual async Task<BaseSearchResponse<FetchDataDto>> GetResourcesAsync(string data, Integration integration)
        {
            var start = DateTime.UtcNow;
            var payload = JsonConvert.DeserializeObject<ApiInformation>(integration.Detail.Content);
            var client = CreateHttpClient(payload, payload.Endpoint);
            var items = await FetchResourceFromIntegrationAsync(client);
            return BaseSearchResponse<FetchDataDto>.CreateFrom(new BaseCriteria() { PageIndex = 0, PageSize = Math.Max(items.Count(), 1) }, (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds, items.Count(), items);
        }
        protected virtual async Task<BaseSearchResponse<FetchDataDto>> GetMetricAsync(string data, Integration integration)
        {
            var start = DateTime.UtcNow;
            var payload = JsonConvert.DeserializeObject<ApiInformation>(integration.Detail.Content);
            var client = CreateHttpClient(payload, payload.BrokerEndpoint);
            var result = await FetchMetricFromIntegrationAsync(client, data);
            return BaseSearchResponse<FetchDataDto>.CreateFrom(new BaseCriteria() { PageIndex = 0, PageSize = Math.Max(result.Count(), 1) }, (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds, result.Count(), result);
        }

        private async Task<IEnumerable<FetchDataDto>> FetchResourceFromIntegrationAsync(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("api/resources?limit=2147483647");
                response.EnsureSuccessStatusCode();

                var responsePayload = await response.ReadJsonContentAsync<WaylayResourceTypeResponse>();
                return responsePayload.Values.Select(x => new FetchDataDto() { Id = x.Id, Name = x.Name });
            }
            catch (JsonException)
            {
                return Array.Empty<FetchDataDto>();
            }
            catch (HttpRequestException)
            {
                throw new SystemCallServiceException(detailCode: MessageConstants.ApiCallMessage.WAYLAY_API_CALL_ERROR);
            }
        }
        private async Task<IEnumerable<FetchDataDto>> FetchMetricFromIntegrationAsync(HttpClient client, string data)
        {
            try
            {
                var response = await client.GetAsync($"resources/{data}/series?metadata");
                response.EnsureSuccessStatusCode();

                return await response.ReadJsonContentAsync<IEnumerable<FetchDataDto>>();
            }
            catch (JsonException)
            {
                return Array.Empty<FetchDataDto>();
            }
            catch (HttpRequestException)
            {
                throw new SystemCallServiceException(detailCode: MessageConstants.ApiCallMessage.WAYLAY_API_CALL_ERROR);
            }
        }

        public virtual async Task<IEnumerable<TimeSeriesDto>> QueryAsync(Integration integration, QueryIntegrationData command)
        {

            var payload = JsonConvert.DeserializeObject<ApiInformation>(integration.Detail.Content);
            var client = CreateHttpClient(payload, payload.BrokerEndpoint);
            var response = await client.GetAsync($"resources/{command.EntityId}/series/{command.MetricKey}?from={command.TimeStart}&until={command.TimeEnd}&aggregate={command.Aggregate}&grouping={command.Grouping}");
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new GenericProcessFailedException(content);
            }
            var body = await response.Content.ReadAsByteArrayAsync();
            var result = body.Deserialize<WaylayQueryResultDataDto>();
            if (result.Series.Any())
            {
                return result.Series.Select(s => new TimeSeriesDto()
                {
                    Timestamp = (long)s[0],
                    Value = s[1],
                });
            }

            return Array.Empty<TimeSeriesDto>();

        }
    }
    public class ApiInformation
    {
        public string Endpoint { get; set; }
        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
        [JsonProperty("api_secret")]
        public string ApiSecret { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; } = "Basic";
        [JsonProperty("broker_endpoint")]
        public string BrokerEndpoint { get; set; }
    }
    public class WaylayResourceTypeResponse
    {
        public IEnumerable<WaylayResourceTypeItemResponse> Values { get; set; }
    }
    public class WaylayResourceTypeItemResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

}
