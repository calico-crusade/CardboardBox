namespace CardboardBox.Http
{
	/// <summary>
	/// Represents a cachable item with fall-back resolver and time to live
	/// </summary>
	/// <typeparam name="T">The type of cachable item</typeparam>
	public class CacheItem<T>
	{
		/// <summary>
		/// The cached item
		/// </summary>
		public T? Cache { get; private set; }

		/// <summary>
		/// The last time the cache was updated
		/// </summary>
		public DateTime? Stamp { get; private set; }

		/// <summary>
		/// How many minutes the cache should live for
		/// </summary>
		public double ExpireMinutes { get; }

		/// <summary>
		/// The resolver to get the latest instance of the cached item
		/// </summary>
		public Func<Task<T>> Resolver { get; }

		/// <summary>
		/// Default constructor for a cache item
		/// </summary>
		/// <param name="resolver">The resolver to get the latest instance of the cached item</param>
		/// <param name="expireMin">The time to live for the cached item (in minutes)</param>
		public CacheItem(Func<Task<T>> resolver, double expireMin = 5)
		{
			Resolver = resolver;
			ExpireMinutes = expireMin;
		}

		/// <summary>
		/// Resolves the item from either the cache or the latest depending on the last time the cache was taken
		/// </summary>
		/// <returns>A task representing the resolved cache item</returns>
		public async Task<T?> Get()
		{
			if (Stamp != null)
			{
				if (Stamp > DateTime.Now)
					return Cache;
			}

			var res = await Resolver();
			Cache = res;
			Stamp = DateTime.Now.AddMinutes(ExpireMinutes);
			return res;
		}
	}
}
