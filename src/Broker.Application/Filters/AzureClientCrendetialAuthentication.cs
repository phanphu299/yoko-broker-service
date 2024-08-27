using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.SharedKernel.Abstraction;
using Broker.Application.Constant;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Broker.Application.Filters
{
    public class AzureClientCrendetialAuthentication : DelegatingHandler
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter<AzureClientCrendetialAuthentication> _logger;

        public AzureClientCrendetialAuthentication(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IConfiguration configuration, ILoggerAdapter<AzureClientCrendetialAuthentication> logger)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await AccquireTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
            HttpResponseMessage result = null;
            try
            {
                result = await base.SendAsync(request, cancellationToken);
            }
            catch (System.Exception exc)
            {
                _logger.LogError("Got exception {Message}, system retry with new access token", exc.Message);
                token = await AccquireTokenAsync(true);
                result = await base.SendAsync(request, cancellationToken);
            }
            return result;
        }

        private async Task<TokenResponse> AccquireTokenAsync(bool clearCache = false)
        {
            var key = "azure_access_token";
            var response = _memoryCache.Get<TokenResponse>(key);
            if (response != null && clearCache == false)
                return response;
            var clientId = _configuration["Azure:ClientId"];
            var clientSecret = _configuration["Azure:ClientSecret"];
            var tenantId = _configuration["Azure:TenantId"];
            var resource = _configuration["Azure:Endpoint"] ?? "https://management.azure.com";
            if (string.IsNullOrEmpty(clientId))
            {
                throw new GenericCommonException(MessageConstants.COMMON_ERROR_MISSED_CONFIG);
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new GenericCommonException(MessageConstants.COMMON_ERROR_MISSED_CONFIG);
            }
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new GenericCommonException(MessageConstants.COMMON_ERROR_MISSED_CONFIG);
            }
            var content = new FormUrlEncodedContent(
            new List<KeyValuePair<string, string>>()
            {
                KeyValuePair.Create("grant_type","client_credentials"),
                KeyValuePair.Create("client_id",clientId),
                KeyValuePair.Create("client_secret",clientSecret),
                KeyValuePair.Create("resource",resource)
            });
            var httpClient = _httpClientFactory.CreateClient("azure-identity-service");
            var tokenRequest = await httpClient.PostAsync($"{tenantId}/oauth2/token", content);
            tokenRequest.EnsureSuccessStatusCode();
            var tokenResponse = await tokenRequest.Content.ReadAsStringAsync();
            response = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse);
            _memoryCache.Set(key, response, TimeSpan.FromSeconds(response.Expired - 300));
            return response;
        }
    }
    public class TokenResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("expires_in")]
        public int Expired { get; set; }
    }
}
