using System.Text.Json;

namespace CardboardBox.Http
{
	public interface IFileCacheService
	{
		/// <summary>
		/// Gets the file either from the cache or from the given URL
		/// </summary>
		/// <param name="url">The url to fetch</param>
		/// <param name="cacheDir">The cache directory, defaults to <see cref="FileCacheService.FILE_CACHE_DIR"/></param>
		/// <returns>A task representing the results of the cached file request</returns>
		Task<(Stream stream, string name, string mimetype)> GetFile(string url, string cacheDir = FileCacheService.FILE_CACHE_DIR);
	}

	public class FileCacheService : IFileCacheService
	{
		public const string FILE_CACHE_DIR = "Cache";
		private readonly IApiService _api;

		public FileCacheService(IApiService api)
		{
			_api = api;
		}

		/// <summary>
		/// Gets the file either from the cache or from the given URL
		/// </summary>
		/// <param name="url">The url to fetch</param>
		/// <param name="cacheDir">The cache directory, defaults to <see cref="FILE_CACHE_DIR"/></param>
		/// <returns>A task representing the results of the cached file request</returns>
		public async Task<(Stream stream, string name, string mimetype)> GetFile(string url, string cacheDir = FILE_CACHE_DIR)
		{
			if (!Directory.Exists(cacheDir))
				Directory.CreateDirectory(cacheDir);

			var hash = url.MD5Hash();

			var cacheInfo = await ReadCacheInfo(hash, cacheDir);
			if (cacheInfo != null)
				return (ReadFile(hash, cacheDir), cacheInfo.Name, cacheInfo.MimeType);


			var io = new MemoryStream();
			var (stream, _, file, type) = await _api.GetData(url);
			await stream.CopyToAsync(io);
			io.Position = 0;
			cacheInfo = new FileCacheItem(file, type, DateTime.Now);
			var worked = await WriteFile(io, hash, cacheDir);
			if (worked)
				await WriteCacheInfo(hash, cacheInfo, cacheDir);
			io.Position = 0;

			return (io, file, type);
		}

		/// <summary>
		/// Gets the path of the cached file from the given cache directory and hash
		/// </summary>
		/// <param name="hash">The file hash</param>
		/// <param name="cacheDir">The cache directory</param>
		/// <returns>The formatted file path</returns>
		public string FilePath(string hash, string cacheDir) => Path.Combine(cacheDir, $"{hash}.data");

		/// <summary>
		/// Gets the path of the cache metadata file from the given cache directory and hash
		/// </summary>
		/// <param name="hash">The file hash</param>
		/// <param name="cacheDir">The cache directory</param>
		/// <returns>The formatted file path</returns>
		public string CachePath(string hash, string cacheDir) => Path.Combine(cacheDir, $"{hash}.cache.json");

		/// <summary>
		/// Reads the given file from the disk
		/// </summary>
		/// <param name="hash">The file hash</param>
		/// <param name="cacheDir">The cache directory</param>
		/// <returns>The file system stream</returns>
		public Stream ReadFile(string hash, string cacheDir)
		{
			var path = FilePath(hash, cacheDir);
			return File.OpenRead(path);
		}

		/// <summary>
		/// Writes the cached file to the file system
		/// </summary>
		/// <param name="stream">The stream to write</param>
		/// <param name="hash">The file hash</param>
		/// <param name="cacheDir">The cache directory</param>
		/// <returns>Whether or not the stream was written correctly</returns>
		public async Task<bool> WriteFile(Stream stream, string hash, string cacheDir)
		{
			try
			{
				var path = FilePath(hash, cacheDir);
				using var io = File.Create(path);
				await stream.CopyToAsync(io);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Reads the cache metadata from the file system
		/// </summary>
		/// <param name="hash">The file hash</param>
		/// <param name="cacheDir">The cache directory</param>
		/// <returns>The cache metadata or null if it wasn't found</returns>
		public async Task<FileCacheItem?> ReadCacheInfo(string hash, string cacheDir)
		{
			var path = CachePath(hash, cacheDir);
			if (!File.Exists(path)) return null;

			using var io = File.OpenRead(path);
			return await JsonSerializer.DeserializeAsync<FileCacheItem>(io);
		}

		/// <summary>
		/// Writes the given file cache information to the file system
		/// </summary>
		/// <param name="hash">The file hash</param>
		/// <param name="item">The file cache metadata to write</param>
		/// <param name="cacheDir">The cache directory</param>
		/// <returns>A task representing the completion of writing the file cache to the file system</returns>
		public async Task WriteCacheInfo(string hash, FileCacheItem item, string cacheDir)
		{
			try
			{
				var path = CachePath(hash, cacheDir);
				using var io = File.Create(path);
				await JsonSerializer.SerializeAsync(io, item);
			}
			catch { }
		}
	}
}
