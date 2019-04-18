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
        public string Host { get; set; }
        public string Password { get; set; }
        public int PageSize { get; set; }
    }
}
