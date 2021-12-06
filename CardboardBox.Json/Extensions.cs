using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CardboardBox.Json
{
	public static class Extensions
	{
		public static IServiceCollection AddJson(this IServiceCollection services, JsonSerializerOptions? options = null)
		{
			return services.AddJson(new SystemTextJsonService(options ?? new JsonSerializerOptions()));
		}

		public static IServiceCollection AddJson<T>(this IServiceCollection services) where T: class, IJsonService
		{
			return services.AddTransient<IJsonService, T>();
		}

		public static IServiceCollection AddJson(this IServiceCollection services, IJsonService json)
		{
			return services.AddSingleton(json);
		}
	}
}
