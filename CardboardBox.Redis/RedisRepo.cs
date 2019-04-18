using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardboardBox.Redis
{
    public interface IRedisRepo
    {
        Task<T> Get<T>(string key, T def = default);
        Task<Dictionary<string, T>> GetAll<T>(string pattern);
        Task<bool> Set<T>(string key, T data);
        Task Subscribe(string channel, Action<RedisChannel, RedisValue> action);
        Task Subscribe<T>(string channel, Action<RedisChannel, T> action);
        Task<bool> Delete(string key);
        Task Publish(string channel, RedisValue message);
        Task Publish<T>(string channel, T result);
    }

    public class RedisRepo : IRedisRepo
    {
        private readonly IRedisConnection redisConnection;
        private readonly IRedisConfig commonConfig;
        private IDatabase Database => redisConnection?.Connection?.GetDatabase();
        private ISubscriber Subscriber => redisConnection?.Connection?.GetSubscriber();

        public RedisRepo(IRedisConnection redisConnection, IRedisConfig commonConfig)
        {
            this.redisConnection = redisConnection;
            this.commonConfig = commonConfig;
        }

        public async Task<T> Get<T>(string key, T def = default)
        {
            var value = await Database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return def;

            return JsonConvert.DeserializeObject<T>(value);
        }

        public async Task<Dictionary<string, T>> GetAll<T>(string pattern)
        {
            var server = redisConnection.Connection.GetServer(redisConnection.Connection.GetEndPoints()[0]);
            var keys = server.Keys(0, pattern, commonConfig.PageSize).ToArray();

            var dic = new Dictionary<string, T>();

            foreach (var key in keys)
                dic.Add(key, await Get<T>(key));

            return dic;
        }

        public async Task<bool> Set<T>(string key, T data)
        {
            var str = JsonConvert.SerializeObject(data);
            return await Database.StringSetAsync(key, str);
        }

        public async Task Subscribe(string channel, Action<RedisChannel, RedisValue> action)
        {
            await Subscriber.SubscribeAsync(channel, action);
        }

        public async Task Subscribe<T>(string channel, Action<RedisChannel, T> action)
        {
            await Subscribe(channel, (c, v) => action(c, JsonConvert.DeserializeObject<T>(v)));
        }

        public async Task<bool> Delete(string key)
        {
            return await Database.KeyDeleteAsync(key);
        }

        public async Task Publish(string channel, RedisValue message)
        {
            await Subscriber.PublishAsync(channel, message);
        }

        public async Task Publish<T>(string channel, T result)
        {
            await Subscriber.PublishAsync(channel, JsonConvert.SerializeObject(result));
        }
    }
}
