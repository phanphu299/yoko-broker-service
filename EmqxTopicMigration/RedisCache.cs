using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EmqxTopicMigration
{
    public class RedisCache
    {
        private readonly Lazy<ConnectionMultiplexer> _lazyConnection;
        private readonly RedisOption _options;

        public RedisCache(RedisOption options)
        {
            _options = options;
            _lazyConnection = new Lazy<ConnectionMultiplexer>((Func<ConnectionMultiplexer>)(() => ConnectionMultiplexer.Connect(options.RedisConnection!)));
        }

        private ConnectionMultiplexer Connection => this._lazyConnection.Value;

        public async Task HashStoreAsync(string key, Dictionary<string, string> hashEntries, TimeSpan? duration, int database)
        {
            var redisDatabase = Connection.GetDatabase(database);
            await redisDatabase.HashSetAsync(key, hashEntries.Select(h => new HashEntry(h.Key, h.Value)).ToArray());
            if(duration != null)
                await redisDatabase.KeyExpireAsync(key, duration);
        }
    }
}