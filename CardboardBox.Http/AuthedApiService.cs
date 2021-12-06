using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CardboardBox.Http
{
	using Json;

	/// <summary>
	/// Represents an <see cref="IApiService"/> with Authorization Tokens added to every request
	/// </summary>
	/// <typeparam name="T">The <see cref="IAuthSettings"/> object to use for this API request</typeparam>
	public class AuthedApiService<T> : ApiService where T : IAuthSettings
	{
		private readonly T _settings;

		public AuthedApiService(
			IHttpClientFactory http,
			IJsonService json,
			ICacheService cache,
			ILogger<ApiService> logger,
			T settings) : base(http, json, cache, logger)
		{
			_settings = settings;
		}

		/// <summary>
		/// Creates an <see cref="IHttpBuilder"/> for the given URL and method
		/// </summary>
		/// <param name="url">The url to request data from</param>
		/// <param name="json">The JSON provider to use for this request</param>
		/// <param name="method">The method used for this HTTP request</param>
		/// <returns>An instance of the <see cref="IHttpBuilder"/></returns>
		public override IHttpBuilder Create(string url, IJsonService json, string method = "GET")
		{
			var scheme = _settings.Scheme;
			if (string.IsNullOrEmpty(scheme))
				scheme = "Bearer";

			return base.Create(url, json, method).Authorization(_settings.Token, scheme);
		}
	}

	/// <summary>
	/// Represents settings used for handling Authorized Api Requests
	/// </summary>
	public interface IAuthSettings
	{
		/// <summary>
		/// The authorization token to use
		/// </summary>
		string Token { get; }

		/// <summary>
		/// The scheme to use for the authorization header
		/// </summary>
		string? Scheme { get; }
	}

	/// <summary>
	/// Represents an abstract implementation of <see cref="IAuthSettings"/>
	/// </summary>
	public abstract class ConfigAuthSettings : IAuthSettings
	{
		private readonly IConfiguration _config;

		/// <summary>
		/// The section of the <see cref="IConfiguration"/> object to use.
		/// </summary>
		public abstract string Section { get; }

		/// <summary>
		/// The authorization token to use
		/// </summary>
		public virtual string Token => _config[Section + ":Token"];
		/// <summary>
		/// The authorization scheme to use (defaults to "Bearer")
		/// </summary>
		public virtual string Scheme => _config[Section + ":Scheme"] ?? "Bearer";

		public ConfigAuthSettings(IConfiguration config)
		{
			_config = config;
		}
	}
}
