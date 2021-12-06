using StackExchange.Redis;

namespace CardboardBox.Redis
{
    using Json;

    public interface IRedisRepo
    {
        Task<T?> Get<T>(string key, T? def = default);
        Task<Dictionary<string, T?>> GetAll<T>(string pattern);
        Task<bool> Set<T>(string key, T data);
        Task Subscribe(string channel, Action<RedisChannel, RedisValue> action);
        Task Subscribe<T>(string channel, Action<RedisChannel, T?> action);
        Task<bool> Delete(string key);
        Task Publish(string channel, RedisValue message);
        Task Publish<T>(string channel, T result);
    }

    public class RedisRepo : IRedisRepo
    {
        private readonly IRedisConnection _con;
        private readonly IRedisConfig _config;
        private readonly IJsonService _json;

        private IDatabase Database => _con.Connection.GetDatabase();
        private ISubscriber Subscriber => _con.Connection.GetSubscriber();

        public RedisRepo(
            IRedisConnection con, 
            IRedisConfig config,
            IJsonService json)
        {
            _con = con;
            _config = config;
            _json = json;
        }

        public async Task<T?> Get<T>(string key, T? def = default)
        {
            var value = await Database.StringGetAsync(key);
            if (value.IsNullOrEmpty) return def;
            return _json.Deserialize<T>(value);
        }

        public async Task<Dictionary<string, T?>> GetAll<T>(string pattern)
        {
            var server = _con.Connection.GetServer(_con.Connection.GetEndPoints()[0]);
            var keys = server.Keys(0, pattern, _config.PageSize).ToArray();

            var dic = new Dictionary<string, T?>();

            foreach (var key in keys)
                dic.Add(key, await Get<T>(key));

            return dic;
        }

        public Task<bool> Set<T>(string key, T data) => Database.StringSetAsync(key, _json.Serialize(data));

        public Task Subscribe(string channel, Action<RedisChannel, RedisValue> action) => Subscriber.SubscribeAsync(channel, action);

        public Task Subscribe<T>(string channel, Action<RedisChannel, T?> action) => Subscribe(channel, (c, v) => action(c, _json.Deserialize<T>(v)));

        public Task<bool> Delete(string key) => Database.KeyDeleteAsync(key);

        public Task Publish(string channel, RedisValue message) => Subscriber.PublishAsync(channel, message);

        public Task Publish<T>(string channel, T result) => Subscriber.PublishAsync(channel, _json.Serialize(result));
    }
}