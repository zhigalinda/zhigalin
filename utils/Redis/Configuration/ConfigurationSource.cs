using Microsoft.Extensions.Configuration;

namespace Redis.Configuration
{
    /// <summary>
    /// Represents a Redis database as an <see cref="IConfigurationSource"/>.
    /// </summary>
    public class ConfigurationSource : IConfigurationSource
    {
        /// <summary>
        /// The redis key.
        /// </summary>
        public string RedisKey { get; set; }
        
        /// <summary>
        /// The redis connection string.
        /// </summary>
        public string RedisConnectionString { get; set; }

        /// <summary>
        /// Builds the <see cref="ConfigurationSource"/> for this source.
        /// </summary>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ConfigurationProvider(RedisKey, RedisConnectionString);
        }
    }
}