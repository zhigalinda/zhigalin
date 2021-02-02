using Redis.Configuration;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Redis
{
    /// <summary>
    /// Extension methods for adding <see cref="Configuration.ConfigurationProvider"/>.
    /// </summary>
    public static class Extensions
    {
        public static string GetChannelTopic(this IDatabase database, string serviceName)
        {
            return $"__keyspace@{database.Database}__:{serviceName}";
        }

        public static IConfigurationBuilder AddRedis(this IConfigurationBuilder builder)
        {
            var existingConfiguration = builder.Build();

            builder.Add(new ConfigurationSource
            {
                RedisKey = existingConfiguration["ServiceName"],
                RedisConnectionString = existingConfiguration.GetConnectionString("Redis")
            });

            return builder;
        }
    }
}