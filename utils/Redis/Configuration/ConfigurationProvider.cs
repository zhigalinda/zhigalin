using System.IO;
using System.Text;
using StackExchange.Redis;

namespace Redis.Configuration
{
    /// <summary>
    /// A Redis based <see cref="ConfigurationProvider"/>.
    /// </summary>
    public class ConfigurationProvider : Microsoft.Extensions.Configuration.ConfigurationProvider
    {
        private readonly string _redisKey;
        private readonly string _redisConnectionString;
        private IDatabase _db;
        private IConnectionMultiplexer _redis;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ConfigurationProvider(string redisKey, string redisConnectionString)
        {
            _redisKey = redisKey;
            _redisConnectionString = redisConnectionString;
        }

        /// <summary>
        /// Loads the configuration data from the redis store.
        /// </summary>
        public override void Load()
        {
            Connect();

            var value = _db.StringGet(_redisKey);

            if (!value.IsNullOrEmpty)
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
                Data = ConfigurationParser.Parse(stream);
            }
        }

        private void Connect()
        {
            if (_redis == null)
            {
                _redis = ConnectionMultiplexer.Connect(_redisConnectionString);
                _db = _redis.GetDatabase(0);

                var subscriber = _redis.GetSubscriber();
                subscriber.Subscribe("__keyspace@0__:" + _redisKey, (channel, value) =>
                {
                    Reload();
                });
            }
        }

        private void Reload()
        {
            Load();
            OnReload();
        }
    }
}