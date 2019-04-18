using StackExchange.Redis;

namespace CardboardBox.Redis
{
    public interface IRedisConnection
    {
        ConnectionMultiplexer Connection { get; }
    }

    public class RedisConnection : IRedisConnection
    {
        private static ConnectionMultiplexer connection;
        private readonly IRedisConfig commonConfig;

        public ConnectionMultiplexer Connection => connection ?? (connection = GetConnection());

        public RedisConnection(IRedisConfig commonConfig)
        {
            this.commonConfig = commonConfig;
        }

        /// <summary>
        /// Fetch a new connection from the configuration file
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnection()
        {
            var options = new ConfigurationOptions
            {
                Password = commonConfig?.Password,
                AllowAdmin = true
            };
            options.EndPoints.Add(commonConfig?.Host ?? "localhost");

            return ConnectionMultiplexer.Connect(options);
        }
    }
}
