using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AHI.Broker.Function.Constant;
using AHI.Broker.Function.Model;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Broker.Function.Service.Model;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Packets;
using Newtonsoft.Json;

namespace AHI.Broker.Function.Service
{
    public class BrokerService : IBrokerService
    {
        private readonly IConfiguration _configuration;
        private readonly ITenantContext _tenantContext;
        private readonly ICache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILoggerAdapter<BrokerService> _logger;
        private const string COAP_TOPIC_PREFIX = "coap/";
        public BrokerService(
            IConfiguration configuration,
            ITenantContext tenantContext,
            ICache cache,
            IHttpClientFactory httpClientFactory,
            ILoggerAdapter<BrokerService> logger)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<bool> CheckMqttAclAsync(CheckMqttAclRequest request)
        {
            if (request.Username == BrokerClientConstants.LISTENER_USER_NAME)
                return true;


            var normalizeTopic = request.Topic;
            if (!string.IsNullOrEmpty(normalizeTopic) && normalizeTopic.StartsWith(COAP_TOPIC_PREFIX))
            {
                normalizeTopic = normalizeTopic.Substring(COAP_TOPIC_PREFIX.Length);
            }

            var key = $"mqtt_acl_checking_{request.Username.CalculateMd5Hash()}";
            var mqttInfos = await _cache.GetAsync<IEnumerable<MqttInfoDto>>(key);

            if (mqttInfos == null)
            {
                await SetTenantContextAsync(request.Username);
                if (string.IsNullOrEmpty(_tenantContext.ProjectId))
                    return false;

                var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
                using (var connection = new SqlConnection(connectionString))
                {
                    var query = @"SELECT [client_id] as Username, [topic_name] as Topic  
                                FROM [emqx_topics] WITH(NOLOCK) 
                                WHERE [deleted] = 0 
                                AND [client_id] = @Username;";
                    await connection.OpenAsync();
                    mqttInfos = await connection.QueryAsync<MqttInfoDto>(query, new { request.Username });
                    await connection.CloseAsync();
                    if (mqttInfos == null || !mqttInfos.Any())
                        return false;

                    await _cache.StoreAsync(key, mqttInfos);
                    _logger.LogDebug("Cache ACL: key={userId},content{content}", key, JsonConvert.SerializeObject(mqttInfos));
                }
            }


            return mqttInfos.Any(x => IsTopicMatched(x.Topic, normalizeTopic)) ||
                   mqttInfos.Any(x => IsTopicMatched(x.Topic, request.Topic));
        }

        private static bool IsTopicMatched(string wildcardPattern, string topic)
        {
            var regexPattern = "^" + Regex.Escape(wildcardPattern)
                .Replace("\\+", "[^/]+")
                .Replace("#", ".+") + "$";
            return Regex.IsMatch(topic, regexPattern);
        }

        public async Task<bool> CheckEmqxAuthenticationAsync(CheckEmqxAuthenticationRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient(HttpClientNames.IDENTITY_FUNCTION);
            var response = await httpClient.PostAsync($"fnc/idp/brokerclients/authenticate", new StringContent(JsonConvert.SerializeObject(new
            {
                request.Username,
                request.Password
            }), Encoding.UTF8, mediaType: "application/json"));
            if (!response.IsSuccessStatusCode)
                return false;

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsByteArrayAsync();
            var authInfo = content.Deserialize<AuthenticationResultDto>();
            return authInfo.Result == "allow";
        }

        public async Task RemoveEmqxBrokersAsync(RemoveEmqxBrokersRequest request)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var success = true;
            var distincIds = request.BrokerIds.Distinct();
            IEnumerable<string> clientIds = null;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var parms = new { brokerIds = distincIds.Select(id => id.ToString()) };
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        const string updateQuery = @"UPDATE [emqx_topics]
                                            SET [deleted] = 1, [updated_utc] = getutcdate()
                                            WHERE [broker_id] IN @brokerIds;";


                        var result = await connection.ExecuteAsync(updateQuery, parms, transaction, 600);
                    }
                    catch (DbException ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        success = false;
                    }

                    if (success)
                    {
                        await transaction.CommitAsync();
                        const string selQuery = @"SELECT distinct [client_id] as Username
                                FROM [emqx_topics] WITH(NOLOCK) 
                                WHERE [deleted] = 1 
                                AND [broker_id] IN @brokerIds";
                        clientIds = await connection.QueryAsync<string>(selQuery, parms);
                        var keys = clientIds.Select(id => $"mqtt_acl_checking_{id.ToString().CalculateMd5Hash()}");
                        var tasks = keys.Select(key => _cache.DeleteAsync(key));
                        await Task.WhenAll(tasks);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }
                }

                await connection.CloseAsync();
            }
        }

        public async Task RemoveClientAsync(RemoveDeviceRequest request)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var success = true;
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var updateQuery = @"UPDATE [emqx_topics]
                                            SET [deleted] = 1, [updated_utc] = getutcdate()
                                            WHERE [broker_id] = @BrokerId
                                            AND [client_id] = @ClientId;";

                        var parms = new { request.BrokerId, request.ClientId };
                        var result = await connection.ExecuteAsync(updateQuery, parms, transaction, 600);
                    }
                    catch (DbException ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        success = false;
                    }

                    if (success)
                    {
                        await transaction.CommitAsync();
                        await TryRemoveCacheAsync(request.ClientId);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                    }
                }

                await connection.CloseAsync();
            }
        }

        public async Task AssignClientAsync(AssignClientRequest request)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            var success = true;

            var topics = new List<string>();
            if (request.Topics != null && request.Topics.Any())
                topics.AddRange(request.Topics);
            if (!string.IsNullOrEmpty(request.Topic))
                topics.Add(request.Topic);

            topics = topics.Where(x => !string.IsNullOrEmpty(x)).Distinct(StringComparer.CurrentCultureIgnoreCase).ToList();
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var query = @"SELECT [topic_name]
                                FROM [emqx_topics] WITH(NOLOCK)
                                WHERE [broker_id] = @BrokerId
                                AND [deleted] = 0
                                AND [client_id] = @ClientId;";
                        var existTopic = await connection.QueryAsync<string>(query, new { request.BrokerId, request.ClientId }, transaction);
                        var insertTopics = topics.Where(x => !existTopic.Contains(x, StringComparer.CurrentCultureIgnoreCase)).ToList();
                        var updateTopics = topics.Where(x => existTopic.Contains(x, StringComparer.CurrentCultureIgnoreCase)).ToList();
                        var deleteTopics = existTopic.Where(x => !topics.Contains(x, StringComparer.CurrentCultureIgnoreCase)).ToList();

                        foreach (var topic in updateTopics)
                        {
                            const string cmd = @"UPDATE [emqx_topics]
                                              SET [access_token] = @AccessToken, [updated_utc] = getutcdate()
                                              WHERE [broker_id] = @BrokerId
                                              AND [topic_name] = @Topic
                                              AND  [deleted] = 0
                                              AND [client_id] = @ClientId;";
                            var parms = new { request.BrokerId, request.ClientId, topic, request.AccessToken };
                            var result = await connection.ExecuteAsync(cmd, parms, transaction, 600);
                        }

                        foreach (var topic in insertTopics)
                        {
                            const string cmd = @"INSERT INTO [emqx_topics] ([broker_id], [client_id], [access_token], [topic_name])
                                              VALUES (@BrokerId, @ClientId, @AccessToken, @Topic);";
                            var parms = new { request.BrokerId, request.ClientId, topic, request.AccessToken };
                            var result = await connection.ExecuteAsync(cmd, parms, transaction, 600);
                        }

                        if(deleteTopics.Any())
                        {
                            const string cmd = @"Update [emqx_topics] set [deleted] = 1 ,[updated_utc] = getutcdate()
                                              WHERE [deleted] = 0
                                              AND [broker_id] = @BrokerId
                                              AND [client_id] = @ClientId
                                              AND [topic_name] in @Topics   ";
                            var parms = new { request.BrokerId, request.ClientId, Topics = deleteTopics};
                            var result = await connection.ExecuteAsync(cmd, parms, transaction, 600);
                        }
                    }
                    catch (DbException ex)
                    {
                        _logger.LogError(ex, ex.Message);
                        success = false;
                    }

                    await (success ? transaction.CommitAsync() : transaction.RollbackAsync());
                }

                await connection.CloseAsync();
            }
            await TryRemoveCacheAsync(request.ClientId);
        }

        private async Task TryRemoveCacheAsync(string clientId)
        {
            var key = $"mqtt_acl_checking_{clientId.CalculateMd5Hash()}";

            try
            {
                await _cache.DeleteAsync(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Remove cache failure. Key={key},Client={clientId}", key, clientId);
            }
        }

        public async Task<IEnumerable<BrokerTopicDto>> GetBrokerTopicsAsync(GetBrokerTopicsRequest request)
        {
            var connectionString = _configuration["ConnectionStrings:Default"].BuildConnectionString(_configuration, _tenantContext.ProjectId);
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"SELECT [client_id] as ClientId, [topic_name] as Topic 
                            FROM [emqx_topics] WITH(NOLOCK) 
                            WHERE [deleted] = 0 
                            AND [broker_id] = @BrokerId;";
                await connection.OpenAsync();
                var result = await connection.QueryAsync<BrokerTopicDto>(query, new { request.BrokerId });
                await connection.CloseAsync();

                return result;
            }
        }

        private async Task SetTenantContextAsync(string username)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(HttpClientNames.IDENTITY);
                var isGuid = Guid.TryParse(username, out var value);
                var response = new HttpResponseMessage();

                if (isGuid)
                {
                    response = await httpClient.GetAsync($"idp/clients/{username}?excludeUserContext=true");
                }
                else
                {
                    response = await httpClient.GetAsync($"idp/brokerclients/{username}");
                }

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsByteArrayAsync();
                var clientInfo = content.Deserialize<BrokerClientDto>();
                _tenantContext.SetProjectId(clientInfo?.ProjectId);
                _tenantContext.SetSubscriptionId(clientInfo?.SubscriptionId);
                _tenantContext.SetTenantId(clientInfo?.TenantId);
            }
            catch
            {
                _tenantContext.SetProjectId(null);
                _tenantContext.SetSubscriptionId(null);
                _tenantContext.SetTenantId(null);
            }
        }
    }
}