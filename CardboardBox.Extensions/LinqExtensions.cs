namespace CardboardBox
{
	public static class LinqExtensions
	{
		#region Randomization

		public static readonly Random RandomInstance = new();

		/// <summary>
		/// Randomly fetches an item from the list, removes it, and returns it.
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="collection">The target collection</param>
		/// <returns>The randomly selected item</returns>
		public static T RandomRemove<T>(this List<T> collection)
		{
			var output = collection.Random();
			collection.Remove(output);
			return output;
		}

		/// <summary>
		/// Fetches a random item from the list
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="collection">The target collection</param>
		/// <returns>The randomly selected item</returns>
		/// <exception cref="ArgumentException">Returned if the collection contains no elements</exception>
		public static T Random<T>(this List<T> collection)
		{
			if (collection.Count == 0) throw new ArgumentException("Collection contains no elements!", nameof(collection));
			if (collection.Count == 1) return collection[0];

			var index = RandomInstance.Next(0, collection.Count);
			return collection[index];
		}

		/// <summary>
		/// Fetches a random item from the array
		/// </summary>
		/// <typeparam name="T">The type of array</typeparam>
		/// <param name="collection">The target array</param>
		/// <returns>The randomly selected item</returns>
		/// <exception cref="ArgumentException">Returned if the collection contains no elements</exception>
		public static T Random<T>(this T[] collection)
		{
			if (collection.Length == 0) throw new ArgumentException("Collection contains no elements!", nameof(collection));
			if (collection.Length == 1) return collection[0];

			var index = RandomInstance.Next(0, collection.Length);
			return collection[index];
		}

		/// <summary>
		/// Fetches a random character from the given string
		/// </summary>
		/// <param name="collection">The string to fetch from</param>
		/// <returns>The randomly selected character</returns>
		public static string Random(this string collection)
		{
			return collection.ToCharArray().Random().ToString();
		}

		#endregion

		#region Dictionary Extensions

		/// <summary>
		/// Groups the given list by the provided key selector and then turns it into a dictionary
		/// </summary>
		/// <typeparam name="T">The type of the dictionaries key</typeparam>
		/// <typeparam name="V">The type of the dictionaries value</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="keySelector">How to determine the key from the collection</param>
		/// <returns>The given collection grouped by the key and sorted into a dictionary</returns>
		public static Dictionary<T, V[]> ToGDictionary<T, V>(this IEnumerable<V> data, Func<V, T> keySelector) where T : notnull
		{
			return data
				.GroupBy(t => keySelector(t))
				.ToDictionary(t => t.Key, t => t.ToArray());
		}

		/// <summary>
		/// Groups the given list by the provided key selector and then turns it into a dictionary.
		/// Sorts the grouped collection by the given ordering function
		/// </summary>
		/// <typeparam name="T">The type of the dictionaries key</typeparam>
		/// <typeparam name="V">The type of the dictionaries value</typeparam>
		/// <typeparam name="O">The return type of the ordering function</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="keySelector">How to determine the key from the collection</param>
		/// <param name="order">How to order each grouped collection</param>
		/// <param name="asc">Whether to order the collection by ascending (default / true) or descending (false)</param>
		/// <returns>The given collection grouped by the key and sorted into a dictionary, ordered by the ordering function</returns>
		public static Dictionary<T, V[]> ToGDictionary<T, V, O>(this IEnumerable<V> data, Func<V, T> keySelector, Func<V, O> order, bool asc = true) where T : notnull
		{
			return data
				.GroupBy(t => keySelector(t))
				.ToDictionary(t => t.Key, t =>
				{
					if (asc) return t.OrderBy(order).ToArray();
					return t.OrderByDescending(order).ToArray();
				});
		}

		/// <summary>
		/// Adds the given item to a dictionary collection, or creates a new collection if one doesn't exist for the given key
		/// </summary>
		/// <typeparam name="TKey">The type of dictionary key</typeparam>
		/// <typeparam name="TValue">The type of collection used as the dictionary value</typeparam>
		/// <param name="dic">The target dictionary</param>
		/// <param name="key">The key of the dictionary</param>
		/// <param name="value">The value to add to the collection</param>
		public static void Add<TKey, TValue>(this Dictionary<TKey, List<TValue>> dic, TKey key, TValue value) where TKey : notnull
		{
			if (!dic.ContainsKey(key))
				dic.Add(key, new List<TValue>());

			dic[key].Add(value);
		}

		/// <summary>
		/// Safely converts the given collection to a dictionary
		/// </summary>
		/// <typeparam name="TKey">The type of key</typeparam>
		/// <typeparam name="TValue">The type of value</typeparam>
		/// <typeparam name="T">The type of data in the collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="key">The key selector</param>
		/// <param name="val">The value selector</param>
		/// <param name="useFirst">Whether or not to use the first matching key or the last</param>
		/// <returns>The key de-duped dictionary</returns>
		public static Dictionary<TKey, TValue> ToDictionarySafe<TKey, TValue, T>(this IEnumerable<T> data, Func<T, TKey> key, Func<T, TValue> val, bool useFirst = true) where TKey: notnull
		{
			var dic = new Dictionary<TKey, TValue>();
			foreach (var item in data)
			{
				var k = key(item);
				var v = val(item);

				if (dic.ContainsKey(k) && useFirst) continue;

				if (dic.ContainsKey(k) && !useFirst)
				{
					dic[k] = v;
					continue;
				}

				dic.Add(k, v);
			}

			return dic;
		}

		/// <summary>
		/// Safely converts the given collection to a dictionary
		/// </summary>
		/// <typeparam name="TKey">The type of ke</typeparam>
		/// <typeparam name="T">The type of data in the collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="key">The key selector</param>
		/// <param name="useFirst">Whether or not to use the first matching key or the last</param>
		/// <returns>The key de-duped dictionary</returns>
		public static Dictionary<TKey, T> ToDictionarySafe<TKey, T>(this IEnumerable<T> data, Func<T, TKey> key, bool useFirst = true) where TKey: notnull
		{
			return data.ToDictionarySafe(key, t => t, useFirst);
		}

		#endregion

		#region Misc

		/// <summary>
		/// Skips the last few elements in the collection
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="count">How many elements to skip (default: 1)</param>
		/// <returns>The collection minus the last few elements</returns>
		public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> data, int count = 1)
		{
			return data.Reverse().Skip(count).Reverse();
		}

		/// <summary>
		/// Performs the given action for every item in the collection
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="action">The action to perform on each element in the collection</param>
		public static void Each<T>(this IEnumerable<T> data, Action<T> action)
		{
			foreach (var item in data)
				action(item);
		}

		/// <summary>
		/// Performs the given action for every item in the collection
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="action">The action to perform on each element in the collection</param>
		public static void Each<T>(this IEnumerable<T> data, Action<int, T> action)
		{
			int count = 0;
			foreach (var item in data)
			{
				action(count, item);
				count++;
			}
		}

		/// <summary>
		/// Performs the given action for every item in the collection when the task is finished
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="tasks">The target collection</param>
		/// <param name="action">The action to perform on each element in the collection</param>
		public static async Task Each<T>(this Task<T[]> tasks, Action<T> action)
		{
			(await tasks).Each(action);
		}

		/// <summary>
		/// Performs the given action for every item in the collection when the task is finished
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="tasks">The target collection</param>
		/// <param name="action">The action to perform on each element in the collection</param>
		public static async Task Each<T>(this Task<T[]> tasks, Action<int, T> action)
		{
			(await tasks).Each(action);
		}

		/// <summary>
		/// Splits the given collection in chunks of a given length
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="chunks">The max number of item per chunk</param>
		/// <returns>The split up chunks of the target collection</returns>
		public static IEnumerable<T[]> Chunk<T>(this IEnumerable<T> data, int chunks)
		{
			var cur = new List<T>();
			int i = 0;
			foreach(var item in data)
			{
				if (i >= chunks)
				{
					yield return cur.ToArray();
					cur.Clear();
				}

				cur.Add(item);
				i++;
			}

			if (cur.Count > 0)
				yield return cur.ToArray();
		}

		/// <summary>
		/// Gets the first item that matches the predicate. If none is found, defaults to <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		/// <typeparam name="T">The type of the records</typeparam>
		/// <param name="data">The collection to iterate through</param>
		/// <param name="prefered">The predicate to match the elements too</param>
		/// <returns>The first prefered item or the results of <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/></returns>
		public static T? PreferedOrFirst<T>(this IEnumerable<T> data, Func<T, bool> prefered)
		{
			foreach (var item in data)
			{
				if (prefered(item)) return item;
			}

			return data.FirstOrDefault();
		}

		#endregion
	}
}