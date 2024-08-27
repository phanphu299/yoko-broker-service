namespace EmqxTopicMigration
{
    public class RedisMigrationCmd
    {
        public string BrokerConnectionString { get; set; }
        public string RedisConnectionString { get; set; }

        public RedisMigrationCmd(string brokerConnectionString, string redisConnectionString)
        {
            BrokerConnectionString = brokerConnectionString;
            RedisConnectionString = redisConnectionString;
        }
    }
}