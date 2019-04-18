using System;

namespace CardboardBox.Redis
{
    using Setup;

    public static class Extensions
    {
        public static IDependencyHandle UseRedis(this IDependencyHandle handle, IRedisConfig config)
        {
            return handle
                    .Use(config)
                    .Use<IRedisConnection, RedisConnection>()
                    .Use<IRedisRepo, RedisRepo>();
        }

        public static IDependencyHandle UseRedis(this IDependencyHandle handle, Func<IRedisConfig> config)
        {
            return handle.UseRedis(config());
        }

        public static IDependencyHandle UseRedis(this IDependencyHandle handle, string host, string password = null, int pagesize = 500000)
        {
            return handle.UseRedis(new RedisConfig
            {
                Host = host,
                Password = password,
                PageSize = pagesize
            });
        }
    }
}
