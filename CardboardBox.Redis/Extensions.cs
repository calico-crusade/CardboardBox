using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox.Redis
{
	using Json;

	public static class Extensions
	{
		public static IServiceCollection AddRedis(this IServiceCollection services, IRedisConfig config)
		{
			return services
				.AddRedisBase()
				.AddSingleton(config);
		}

		public static IServiceCollection AddRedis<T>(this IServiceCollection services) where T: class, IRedisConfig
		{
			return services
				.AddRedisBase()
				.AddTransient<IRedisConfig, T>();
		}

		public static IServiceCollection AddRedis(this IServiceCollection services, string host, string password, int pageSize = 500000)
		{
			return services.AddRedis(new RedisConfigSettings(host, password, pageSize));
		}

		public static IServiceCollection AddRedis(this IServiceCollection services)
		{
			return services.AddRedis<RedisConfig>();
		}

		private static IServiceCollection AddRedisBase(this IServiceCollection services)
		{
			return services
				.AddJson()
				.AddSingleton<IRedisConnection, RedisConnection>()
				.AddTransient<IRedisRepo, RedisRepo>();
		}
	}
}
