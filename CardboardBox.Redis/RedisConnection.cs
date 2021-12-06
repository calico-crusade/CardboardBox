using StackExchange.Redis;

namespace CardboardBox.Redis
{
    public interface IRedisConnection
    {
        ConnectionMultiplexer Connection { get; }
    }

    public class RedisConnection : IRedisConnection
    {
        private readonly IRedisConfig _config;
        private ConnectionMultiplexer? _connection;

        public ConnectionMultiplexer Connection => _connection ??= GetConnection();

        public RedisConnection(IRedisConfig commonConfig)
        {
            _config = commonConfig;
        }

        /// <summary>
        /// Fetch a new connection from the configuration file
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnection()
        {
            var options = new ConfigurationOptions
            {
                Password = _config?.Password,
                AllowAdmin = true
            };
            options.EndPoints.Add(_config?.Host ?? "localhost");

            return ConnectionMultiplexer.Connect(options);
        }
    }
}
