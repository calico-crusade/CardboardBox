using Microsoft.Extensions.Configuration;

namespace CardboardBox.Redis
{
    public interface IRedisConfig
    {
        string Host { get; }
        string Password { get; }
        int PageSize { get; }
    }

    public class RedisConfig : IRedisConfig
    {
        private readonly IConfiguration _config;

        public RedisConfig(IConfiguration config)
		{
            _config = config;
		}

        public string Host => _config["Redis:Host"];
        public string Password => _config["Reids:Password"];
        public int PageSize => int.TryParse(_config["Redis:PageSize"], out int size) ? size : 500000;
    }

    public class RedisConfigSettings : IRedisConfig
	{
        public string Host { get; }
        public string Password { get; }
        public int PageSize { get; }

        public RedisConfigSettings(string host, string password, int pageSize = 500000)
		{
            Host = host;
            Password = password;
            PageSize = pageSize;
		}
	}
}
