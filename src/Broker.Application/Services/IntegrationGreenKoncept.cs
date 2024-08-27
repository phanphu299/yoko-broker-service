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
    public class IntegrationGreenKoncept : IIntegrationHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private static IDictionary<string, HttpClient> _cacheHttpClient = new Dictionary<string, HttpClient>();
        private readonly IDictionary<string, Func<string, Integration, Task<BaseSearchResponse<FetchDataDto>>>> _methods;

        public IntegrationGreenKoncept(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _methods = new Dictionary<string, Func<string, Integration, Task<BaseSearchResponse<FetchDataDto>>>>()
            {
                { IntegrationTypeConstants.GREEN_KONCEPT_NODE_FETCH_TYPE, GetGreenKonceptMetadataAsync },
                { IntegrationTypeConstants.GREEN_KONCEPT_MEASURE_FETCH_TYPE, GetGreenKonceptMeasuresAsync },
                // support FE call with the same parameters as WAYLAY integration
                { IntegrationTypeConstants.WAY_LAY_DEVICE_FETCH_TYPE, GetGreenKonceptMetadataAsync },
                { IntegrationTypeConstants.WAY_LAY_METRIC_FETCH_TYPE, GetGreenKonceptMeasuresAsync },
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
        private async Task<HttpClient> CreateHttpClientAsync(GreenKonceptInfomation payload, string baseAddress)
        {
            //if (_cacheHttpClient.ContainsKey(baseAddress))
            //{
            //    return _cacheHttpClient[baseAddress];
            //}
            if (!string.IsNullOrEmpty(payload.Endpoint))
            {
                try
                {
                    var httpClientToken = _httpClientFactory.CreateClient(baseAddress);
                    httpClientToken.BaseAddress = new Uri(baseAddress);

                    //get token
                    httpClientToken.DefaultRequestHeaders.Clear();
                    var content = new Dictionary<string, string>
                    {
                        ["clientId"] = payload.ClientId,
                        ["clientSecret"] = payload.ClientSecret
                    };
                    var responseMessage = await httpClientToken.PostAsync($"api/v1/auth/token", new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json"));

                    // if (!responseMessage.IsSuccessStatusCode)
                    // {
                    //     var response = await responseMessage.Content.ReadAsStringAsync();
                    //     throw new GenericProcessFailedException($"Can not get token {response}");
                    // }

                    var message = await responseMessage.Content.ReadAsByteArrayAsync();
                    var accessToken = message.Deserialize<GreenKonceptTokenRespone>()?.AccessToken;

                    //create client
                    var httpClient = _httpClientFactory.CreateClient(baseAddress);
                    httpClient.BaseAddress = new Uri(baseAddress);
                    httpClient.DefaultRequestHeaders.Clear();

                    //fake token for measures api
                    //accessToken = "eyJhbGciOiJIUzUxMiJ9.eyJzdWIiOiJZb2tvZ2F3YUVFTVMxIiwidG9rZW4tc2NvcGUiOiJBQ0NFU1NfVE9LRU4iLCJ1c2VyLWFjY291bnQtaWQiOjE5Miwic2NvcGVzIjoibnVsbCx2aWV3OmlvdCx2aWV3OnNldHRpbmdzLHZpZXc6cmVwb3J0cyx2aWV3OmVuZXJneSx2aWV3OmlhcSx2aWV3OmZhdWx0ZGIiLCJleHAiOjE2NTY2NjY4NzIsInVzZXItYWNjb3VudC1uYW1lIjoieW9rb2dhd2Ffc2ciLCJ1c2VyLXN1Yi1hY2NvdW50LWlkIjpudWxsLCJpYXQiOjE2NTU3Nzc4Mzl9.DV-vJzaAkpuNSogdyBdqEQRKFdOabpgh4brBf1_rjmnghV8jQ8U9hFCRvNkAGavPGPESnKHdX1RvEJYH1woJaA";

                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                    //_cacheHttpClient[baseAddress] = httpClient;
                    return httpClient;
                }
                catch (HttpRequestException)
                {
                    throw new SystemCallServiceException(detailCode: MessageConstants.ApiCallMessage.GREEN_KONCEPT_API_CALL_ERROR);
                }

            }
            throw new GenericProcessFailedException("Can not create the httpclient because the endpoint is not defined");
        }
        protected virtual async Task<BaseSearchResponse<FetchDataDto>> GetGreenKonceptMetadataAsync(string data, Integration integration)
        {
            var start = DateTime.UtcNow;
            var payload = JsonConvert.DeserializeObject<GreenKonceptInfomation>(integration.Detail.Content);
            var client = await CreateHttpClientAsync(payload, payload.Endpoint);
            var items = await FetchGreenKonceptMetadataFromIntegrationAsync(client);
            return BaseSearchResponse<FetchDataDto>.CreateFrom(new BaseCriteria() { PageIndex = 0, PageSize = Math.Max(items.Count(), 1) }, (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds, items.Count(), items);
        }
        protected virtual async Task<BaseSearchResponse<FetchDataDto>> GetGreenKonceptMeasuresAsync(string data, Integration integration)
        {
            var start = DateTime.UtcNow;
            var payload = JsonConvert.DeserializeObject<GreenKonceptInfomation>(integration.Detail.Content);
            var client = await CreateHttpClientAsync(payload, payload.Endpoint);
            var result = await FetchMeasuresFromIntegrationAsync(client, data);
            return BaseSearchResponse<FetchDataDto>.CreateFrom(new BaseCriteria() { PageIndex = 0, PageSize = Math.Max(result.Count(), 1) }, (long)DateTime.UtcNow.Subtract(start).TotalMilliseconds, result.Count(), result);
        }
        private async Task<IEnumerable<FetchDataDto>> FetchGreenKonceptMetadataFromIntegrationAsync(HttpClient client)
        {
            try
            {
                var response = await client.GetAsync("api/v1/iot/node-hierarchy");
                response.EnsureSuccessStatusCode();

                var responsePayload = await response.ReadJsonContentAsync<GreenKonceptMetadata>();
                var result = new List<GreenKonceptMetadata>
                {
                    new GreenKonceptMetadata { NodeName = responsePayload.NodeName, NodeUID = responsePayload.NodeUID }
                };
                ProcessGetAllRealNodeGreenKoncept(responsePayload.Children, result);
                return result.Select(x => new FetchDataDto() { Id = x.NodeUID.ToString(), Name = x.NodeName });
            }
            catch (JsonException)
            {
                return Array.Empty<FetchDataDto>();
            }
            catch (HttpRequestException)
            {
                throw new SystemCallServiceException(detailCode: MessageConstants.ApiCallMessage.GREEN_KONCEPT_API_CALL_ERROR);
            }
        }
        private async Task<IEnumerable<FetchDataDto>> FetchMeasuresFromIntegrationAsync(HttpClient client, string nodeUid)
        {
            try
            {
                var response = await client.GetAsync($"api/v1/iot/node/{nodeUid}/measures");
                response.EnsureSuccessStatusCode();

                var responsePayload = await response.ReadJsonContentAsync<IEnumerable<GreenKonceptMeasureData>>();
                return responsePayload.Select(x => new FetchDataDto() { Id = x.MeasureUID, Name = x.DataName, Items = x.MeasureUnit });
            }
            catch (JsonException)
            {
                return Array.Empty<FetchDataDto>();
            }
            catch (HttpRequestException)
            {
                throw new SystemCallServiceException(detailCode: MessageConstants.ApiCallMessage.GREEN_KONCEPT_API_CALL_ERROR);
            }
        }
        protected virtual bool CanFetch(Integration integration)
        {
            return string.Equals(IntegrationTypeConstants.GREEN_KONCEPT, integration.Type, StringComparison.InvariantCultureIgnoreCase);
        }
        public virtual async Task<IEnumerable<TimeSeriesDto>> QueryAsync(Integration integration, QueryIntegrationData command)
        {
            // query the data 
            var payload = JsonConvert.DeserializeObject<GreenKonceptInfomation>(integration.Detail.Content);
            var client = await CreateHttpClientAsync(payload, payload.Endpoint);
            var response = await client.GetAsync($"api/v1/iot/data-events?nodeuids={command.EntityId}&start-date={command.TimeStart}&end-date={command.TimeEnd}&bin={command.Grouping}&measures={command.MetricKey}");
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new GenericProcessFailedException(content);
            }
            var body = await response.Content.ReadAsByteArrayAsync();
            var result = body.Deserialize<IEnumerable<GreenKonceptSeries>>();
            return result.FirstOrDefault()?.Events ?? new List<TimeSeriesDto>();
        }

        private void ProcessGetAllRealNodeGreenKoncept(ICollection<GreenKonceptMetadata> metadata, ICollection<GreenKonceptMetadata> ouput)
        {
            foreach (var data in metadata)
            {
                if (data.Children != null && data.Children.Any())
                    ProcessGetAllRealNodeGreenKoncept(data.Children, ouput);

                //if (string.IsNullOrEmpty(data.NodeDeviceID))
                //    continue;

                var temp = data;
                temp.Children = null;
                ouput.Add(temp);
            }
        }
    }
    public class GreenKonceptTokenRespone
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
    }
    public class GreenKonceptSeries
    {
        public string NodeName { get; set; }
        public string NodeId { get; set; }
        public IEnumerable<TimeSeriesDto> Events { get; set; }
    }
    public class GreenKonceptMeasureData
    {
        public string DataName { get; set; }
        public string MeasureUID { get; set; }
        public string MeasureUnit { get; set; }
    }
    public class GreenKonceptMetadata
    {
        public string NodeName { get; set; }
        public double NodeUID { get; set; }
        public string NodeID { get; set; }
        public string LocationType { get; set; }
        public string Resource { get; set; }
        public string ResourceType { get; set; }
        public string NodeDeviceID { get; set; }
        public virtual ICollection<GreenKonceptMetadata> Children { get; set; }
    }

}
