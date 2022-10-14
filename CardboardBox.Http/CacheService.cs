using System.Security.Cryptography;
using System.Text;

namespace CardboardBox.Http
{
	using Json;

	/// <summary>
	/// Exposes the underlying caching mechanism for Cardboard HTTP
	/// </summary>
	public interface ICacheService
	{
		/// <summary>
		/// Fetches the data from the cache
		/// </summary>
		/// <typeparam name="T">The type of item to fetch</typeparam>
		/// <param name="filename">The name of the cached item</param>
		/// <returns>A task representing the loaded data</returns>
		Task<T?> Load<T>(string filename);

		/// <summary>
		/// Saves the data to the cache
		/// </summary>
		/// <typeparam name="T">The type of item to save</typeparam>
		/// <param name="data">The data to save</param>
		/// <param name="filename">The name of the cached item</param>
		/// <returns>A task representing the completion of the saved cache item</returns>
		Task Save<T>(T data, string filename);

		/// <summary>
		/// Validates whether or not the given cache item is valid
		/// </summary>
		/// <param name="uri">The URI to latest copy of the resource</param>
		/// <param name="filename">The outputted filename</param>
		/// <param name="cacheDir">The cache directory to use</param>
		/// <param name="cacheMin">The time to live for the cache</param>
		/// <returns>Whether or not the cached item is valid.</returns>
		bool Validate(string uri, out string? filename, string cacheDir = "Cache", double cacheMin = 5);
	}

	/// <summary>
	/// Represents a file-system based <see cref="ICacheService"/>
	/// </summary>
	public class DiskCacheService : ICacheService
	{
		private readonly IJsonService _json;

		public DiskCacheService(IJsonService json)
		{
			_json = json;
		}

		/// <summary>
		/// Fetches the data from the cache
		/// </summary>
		/// <typeparam name="T">The type of item to fetch</typeparam>
		/// <param name="filename">The name of the cached item</param>
		/// <returns>A task representing the loaded data</returns>
		public async Task<T?> Load<T>(string filename)
		{
			using var stream = File.OpenRead(filename);
			return await _json.Deserialize<T>(stream);
		}

		/// <summary>
		/// Saves the data to the cache
		/// </summary>
		/// <typeparam name="T">The type of item to save</typeparam>
		/// <param name="data">The data to save</param>
		/// <param name="filename">The name of the cached item</param>
		/// <returns>A task representing the completion of the saved cache item</returns>
		public Task Save<T>(T data, string filename)
		{
			var str = _json.Serialize(data);
			return File.WriteAllTextAsync(filename, str);
		}

		/// <summary>
		/// Validates whether or not the given cache item is valid
		/// </summary>
		/// <param name="uri">The URI to latest copy of the resource</param>
		/// <param name="filename">The outputted filename</param>
		/// <param name="cacheDir">The cache directory to use</param>
		/// <param name="cacheMin">The time to live for the cache</param>
		/// <returns>Whether or not the cached item is valid.</returns>
		public bool Validate(string uri, out string? filename, string cacheDir = "Cache", double cacheMin = 5)
		{
			filename = null;
			if (string.IsNullOrEmpty(uri))
				return false;

			if (!Directory.Exists(cacheDir))
				Directory.CreateDirectory(cacheDir);

			filename = Path.Combine(cacheDir, Hash(uri));
			if (!File.Exists(filename))
				return false;

			var modTime = File.GetLastWriteTime(filename);
			var expires = modTime.AddMinutes(cacheMin);

			return expires > DateTime.Now;
		}

		/// <summary>
		/// Hashes the given value to MD5
		/// </summary>
		/// <param name="data">The value to hash</param>
		/// <returns>The hashed value</returns>
		public virtual string Hash(string data)
		{
			using var md5 = MD5.Create();
			var bytes = Encoding.UTF8.GetBytes(data);
			var hash = md5.ComputeHash(bytes);
			return string.Join("", hash.Select(t => t.ToString("X2")));
		}
	}
}
