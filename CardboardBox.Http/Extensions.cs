using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace CardboardBox.Http
{
	using Json;

	/// <summary>
	/// Extensions that include Cardboard HTTP in the given <see cref="IServiceCollection"/>
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Adds Cardboard HTTP with the given JSON provider
		/// </summary>
		/// <param name="services">The service collection to add Cardboard HTTP to</param>
		/// <param name="json">The json provider</param>
		/// <returns>The referenced service provider for chaining</returns>
		public static IServiceCollection AddCardboardHttp(this IServiceCollection services, IJsonService json)
		{
			return services
				.AddCardboardHttpBase()
				.AddJson(json);
		}

		/// <summary>
		/// Adds Cardboard HTTP with the given JSON provider
		/// </summary>
		/// <typeparam name="T">The type of JSON provider to add</typeparam>
		/// <param name="services">The service collection to add Cardboard HTTP to</param>
		/// <returns>The referenced service provider for chaining</returns>
		public static IServiceCollection AddCardboardHttp<T>(this IServiceCollection services) where T: class, IJsonService
		{
			return services
				.AddCardboardHttpBase()
				.AddJson<T>();
		}

		/// <summary>
		/// Adds Cardboard HTTP with the given JSON provider
		/// </summary>
		/// <param name="services">The service collection to add Cardboard HTTP to</param>
		/// <param name="options">Any optional options for the <see cref="JsonSerializer"/></param>
		/// <returns>The referenced service provider for chaining</returns>
		public static IServiceCollection AddCardboardHttp(this IServiceCollection services, JsonSerializerOptions? options = null)
		{
			return services
				.AddCardboardHttpBase()
				.AddJson(options);
		}

		/// <summary>
		/// Registers all of the base services necessary for Cardboard HTTP
		/// </summary>
		/// <param name="services">The service collection to add Cardboard HTTP to</param>
		/// <returns>The referenced service provider for chaining</returns>
		private static IServiceCollection AddCardboardHttpBase(this IServiceCollection services)
		{
			return services
				.AddHttpClient()
				.AddTransient<IApiService, ApiService>()
				.AddTransient<ICacheService, FileCacheService>();
		}
	}
}
