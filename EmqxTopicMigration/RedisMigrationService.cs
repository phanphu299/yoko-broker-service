using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace EmqxTopicMigration
{
    public class RedisMigrationService
    {
        private readonly ILogger<RedisMigrationService> _logger;

        public RedisMigrationService(ILogger<RedisMigrationService> logger)
        {
            _logger = logger;
        }

        public async Task Migrate(RedisMigrationCmd cmd)
        {
            var topics = await GetTopicAuthenInfos(cmd.BrokerConnectionString);
            var tasks = new List<Task>();
            var redisOption = new RedisOption()
            {
                RedisConnection = cmd.RedisConnectionString
            };
            var redisCache = new RedisCache(redisOption);
            foreach (var topicAuthenInfo in topics)
            {
                tasks.Add(AddRedis(redisCache, topicAuthenInfo));
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Number brokers: {numberBroker}", topics.Count());
        }

        private async Task<IEnumerable<TopicAuthenInfo>> GetTopicAuthenInfos(string brokerConnectionString)
        {
            const string query = @"select client_id Username, access_token Password, topic_name Topic
                                   from emqx_topics
                                   where deleted = 0 and topic_name is not null";

            using var connection = new SqlConnection(brokerConnectionString);
            var topics = await connection.QueryAsync<QueryResult>(query);
            return topics.GroupBy(x => new { x.Username, x.Password })
                .Select(g =>
                {
                    return new TopicAuthenInfo()
                    {
                        Username = g.Key.Username,
                        Token = g.Key.Password,
                        Topic = g.Select(c => c.Topic)
                    };
                });
        }

        private async Task AddRedis(RedisCache redisCache, TopicAuthenInfo authenInfo)
        {
            var duration = TimeSpan.FromDays(365);
            var tasks = new List<Task>
            {
                redisCache.HashStoreAsync($"mqtt_user:{authenInfo.Username}", new Dictionary<string, string>()
                {
                    { "password", authenInfo.Token! }
                }, null, 0)
            };

            tasks.AddRange(authenInfo.Topic!.Select(s => redisCache.HashStoreAsync($"mqtt_acl:{authenInfo.Username}", new Dictionary<string, string>() { { s!, "all" } }, null, 0)));
            await Task.WhenAll(tasks);
        }

        private class QueryResult
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Topic { get; set; }
        }
    }
}