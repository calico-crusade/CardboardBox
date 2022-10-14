using Flurl;
using Microsoft.Extensions.Logging;

namespace CardboardBox.Http
{
	using Json;

	/// <summary>
	/// Exposes some common HTTP methods using <see cref="IHttpClientFactory"/> with built in features like caching, authorization, JSON deserializing, etc.
	/// </summary>
	public interface IApiService
	{
		/// <summary>
		/// Creates an <see cref="IHttpBuilder"/> for the given URL and method
		/// </summary>
		/// <param name="url">The url to request data from</param>
		/// <param name="method">The method used for this HTTP request</param>
		/// <returns>An instance of the <see cref="IHttpBuilder"/></returns>
		IHttpBuilder Create(string url, string method = "GET");

		/// <summary>
		/// Creates an <see cref="IHttpBuilder"/> for the given URL and method
		/// </summary>
		/// <param name="url">The url to request data from</param>
		/// <param name="json">The JSON provider to use for this request</param>
		/// <param name="method">The method used for this HTTP request</param>
		/// <returns>An instance of the <see cref="IHttpBuilder"/></returns>
		IHttpBuilder Create(string url, IJsonService json, string method = "GET");

		#region GET requests

		/// <summary>
		/// Creates a GET request for the given URL
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		Task<T?> Get<T>(string url, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a GET request for the given URL
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">The return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> Get<TSuccess, TFailure>(string url, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a GET request for the given URL with a time-based caching layer
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <param name="cacheDir">The directory to store the request cache in</param>
		/// <param name="cacheMin">How long to store the cache for</param>
		/// <returns>A Task representing the results of the request</returns>
		Task<T?> CacheGet<T>(string url, Action<HttpRequestMessage>? config = null, string? cacheDir = null, double? cacheMin = null);

		/// <summary>
		/// Creates a GET request for the given URL
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">The return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <param name="cacheDir">The directory to store the request cache in</param>
		/// <param name="cacheMin">How long to store the cache for</param>
		/// <returns>A Task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> CacheGet<TSuccess, TFailure>(string url, Action<HttpRequestMessage>? config = null, string? cacheDir = null, double? cacheMin = null);

		#endregion

		#region POST requests

		/// <summary>
		/// Creates a POST request for the given URL and data
		/// </summary>
		/// <typeparam name="TResult">The return type</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<TResult?> Post<TResult, TData>(string url, TData data, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a POST request for the given URL and data
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> Post<TSuccess, TFailure, TData>(string url, TData data, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a POST request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="formData">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<T?> Post<T>(string url, (string key, string val)[] formData, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a POST request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> Post<TSuccess, TFailure>(string url, (string key, string val)[] data, Action<HttpRequestMessage>? config = null);

		#endregion

		#region PUT requests

		/// <summary>
		/// Creates a PUT request for the given URL and data
		/// </summary>
		/// <typeparam name="TResult">The return type</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the PUT request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<TResult?> Put<TResult, TData>(string url, TData data, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a PUT request for the given URL and data
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the PUT request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> Put<TSuccess, TFailure, TData>(string url, TData data, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a PUT request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="formData">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<T?> Put<T>(string url, (string key, string val)[] formData, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a PUT request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the PUT request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> Put<TSuccess, TFailure>(string url, (string key, string val)[] data, Action<HttpRequestMessage>? config = null);
		
		#endregion

		#region DELETE requests

		/// <summary>
		/// Creates a DELETE request for the given URL
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		Task<T?> Delete<T>(string url, Action<HttpRequestMessage>? config = null);

		/// <summary>
		/// Creates a DELETE request for the given URL
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">The return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		Task<HttpStatusResult<TSuccess, TFailure>> Delete<TSuccess, TFailure>(string url, Action<HttpRequestMessage>? config = null);
		
		#endregion

		/// <summary>
		/// Generates a URL with the given query parameters
		/// </summary>
		/// <param name="uri">The base URI to use</param>
		/// <param name="pars">All of the query parameters to append to the URI</param>
		/// <returns>The fully qualified URI</returns>
		string GenerateUri(string uri, params (string, string)[] pars);

		/// <summary>
		/// Generates a URL with the given query parameters
		/// </summary>
		/// <param name="parts">The parts of the base URI</param>
		/// <param name="pars">All of the query parameters to append to the URI</param>
		/// <returns>The fully qualified URI</returns>
		string GenerateUri(string[] parts, params (string, string)[] pars);
	}

	/// <summary>
	/// The concrete implementation of <see cref="IApiService"/>
	/// </summary>
	public class ApiService : IApiService
	{
		private readonly IHttpClientFactory _httpFactory;
		private readonly IJsonService _json;
		private readonly ICacheService _cache;
		private readonly ILogger _logger;

		public ApiService(
			IHttpClientFactory httpFactory,
			IJsonService json,
			ICacheService cache, 
			ILogger<ApiService> logger)
		{
			_httpFactory = httpFactory;
			_json = json;
			_cache = cache;
			_logger = logger;
		}

		/// <summary>
		/// Creates an <see cref="IHttpBuilder"/> for the given URL and method
		/// </summary>
		/// <param name="url">The url to request data from</param>
		/// <param name="json">The JSON provider to use for this request</param>
		/// <param name="method">The method used for this HTTP request</param>
		/// <returns>An instance of the <see cref="IHttpBuilder"/></returns>
		public virtual IHttpBuilder Create(string url, IJsonService json, string method = "GET") => new HttpBuilder(_httpFactory, json, _cache, _logger).Method(method).Uri(url).FailGracefully();

		/// <summary>
		/// Creates an <see cref="IHttpBuilder"/> for the given URL and method
		/// </summary>
		/// <param name="url">The url to request data from</param>
		/// <param name="method">The method used for this HTTP request</param>
		/// <returns>An instance of the <see cref="IHttpBuilder"/></returns>
		public virtual IHttpBuilder Create(string url, string method = "GET") => Create(url, _json, method);

		#region GET Requests

		/// <summary>
		/// Creates a GET request for the given URL
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		public Task<T?> Get<T>(string url, Action<HttpRequestMessage>? config = null)
		{
			return Create(url).With(config).Result<T>();
		}

		/// <summary>
		/// Creates a GET request for the given URL
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">The return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> Get<TSuccess, TFailure>(string url, Action<HttpRequestMessage>? config = null)
		{
			return Create(url).With(config).Result<TSuccess, TFailure>();
		}

		/// <summary>
		/// Creates a GET request for the given URL with a time-based caching layer
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <param name="cacheDir">The directory to store the request cache in</param>
		/// <param name="cacheMin">How long to store the cache for</param>
		/// <returns>A Task representing the results of the request</returns>
		public Task<T?> CacheGet<T>(string url, Action<HttpRequestMessage>? config = null, string? cacheDir = null, double? cacheMin = null)
		{
			return Create(url).With(config).Cache(true, cacheDir, cacheMin).Result<T>();
		}

		/// <summary>
		/// Creates a GET request for the given URL
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">The return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <param name="cacheDir">The directory to store the request cache in</param>
		/// <param name="cacheMin">How long to store the cache for</param>
		/// <returns>A Task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> CacheGet<TSuccess, TFailure>(string url, Action<HttpRequestMessage>? config = null, string? cacheDir = null, double? cacheMin = null)
		{
			return Create(url).With(config).Cache(true, cacheDir, cacheMin).Result<TSuccess, TFailure>();
		}
		
		#endregion

		#region POST requests
		
		/// <summary>
		/// Creates a POST request for the given URL and data
		/// </summary>
		/// <typeparam name="TResult">The return type</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<TResult?> Post<TResult, TData>(string url, TData data, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "POST").With(config).Body(data).Result<TResult>();
		}

		/// <summary>
		/// Creates a POST request for the given URL and data
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> Post<TSuccess, TFailure, TData>(string url, TData data, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "POST").With(config).Body(data).Result<TSuccess, TFailure>();
		}

		/// <summary>
		/// Creates a POST request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="formData">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<T?> Post<T>(string url, (string key, string val)[] formData, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "POST").With(config).Body(formData).Result<T>();
		}

		/// <summary>
		/// Creates a POST request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> Post<TSuccess, TFailure>(string url, (string key, string val)[] data, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "POST").With(config).Body(data).Result<TSuccess, TFailure>();
		}
		
		#endregion

		#region PUT requests
		
		/// <summary>
		/// Creates a PUT request for the given URL and data
		/// </summary>
		/// <typeparam name="TResult">The return type</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the PUT request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<TResult?> Put<TResult, TData>(string url, TData data, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "PUT").With(config).Body(data).Result<TResult>();
		}

		/// <summary>
		/// Creates a PUT request for the given URL and data
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <typeparam name="TData">The data type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the PUT request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> Put<TSuccess, TFailure, TData>(string url, TData data, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "PUT").With(config).Body(data).Result<TSuccess, TFailure>();
		}

		/// <summary>
		/// Creates a PUT request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="formData">The body of the POST request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<T?> Put<T>(string url, (string key, string val)[] formData, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "PUT").With(config).Body(formData).Result<T>();
		}

		/// <summary>
		/// Creates a PUT request for the given URL and data.
		/// Data will be encoded using <see cref="FormUrlEncodedContent"/>
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">he return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="data">The body of the PUT request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> Put<TSuccess, TFailure>(string url, (string key, string val)[] data, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "PUT").With(config).Body(data).Result<TSuccess, TFailure>();
		}
		
		#endregion

		#region DELETE requests
		/// <summary>
		/// Creates a DELETE request for the given URL
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		public Task<T?> Delete<T>(string url, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "DELETE").With(config).Result<T>();
		}

		/// <summary>
		/// Creates a DELETE request for the given URL
		/// </summary>
		/// <typeparam name="TSuccess">The return type for a successful request</typeparam>
		/// <typeparam name="TFailure">The return type for a failed request</typeparam>
		/// <param name="url">The URL of the request</param>
		/// <param name="config">Any optional configuration necessary</param>
		/// <returns>A Task representing the results of the request</returns>
		public Task<HttpStatusResult<TSuccess, TFailure>> Delete<TSuccess, TFailure>(string url, Action<HttpRequestMessage>? config = null)
		{
			return Create(url, "DELETE").With(config).Result<TSuccess, TFailure>();
		}
		#endregion

		/// <summary>
		/// Generates a URL with the given query parameters
		/// </summary>
		/// <param name="uri">The base URI to use</param>
		/// <param name="pars">All of the query parameters to append to the URI</param>
		/// <returns>The fully qualified URI</returns>
		public string GenerateUri(string uri, params (string, string)[] pars)
		{
			foreach (var (k, v) in pars)
				uri = uri.SetQueryParam(k, v);

			return uri;
		}

		/// <summary>
		/// Generates a URL with the given query parameters
		/// </summary>
		/// <param name="parts">The parts of the base URI</param>
		/// <param name="pars">All of the query parameters to append to the URI</param>
		/// <returns>The fully qualified URI</returns>
		public string GenerateUri(string[] parts, params (string, string)[] pars)
		{
			var uri = string.Join("/",
				parts.Select(t => t.EndsWith("/") ? t.Remove(t.Length - 1, 1) : t));

			return GenerateUri(uri, pars);
		}
	}
}
