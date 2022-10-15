namespace CardboardBox
{
	public static class AsyncLinqExtensions
	{
		/// <summary>
		/// Performs the given action for every item in the collection
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="action">The action to perform on each element in the collection</param>
		public static Task Each<T>(this IAsyncEnumerable<T> data, Action<T> action) => data.Each((i, t) => action(t));

		/// <summary>
		/// Performs the given action for every item in the collection
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="action">The action to perform on each element in the collection</param>
		public static async Task Each<T>(this IAsyncEnumerable<T> data, Action<int, T> action)
		{
			int count = 0;
			await foreach (var item in data)
			{
				action(count, item);
				count++;
			}
		}

		/// <summary>
		/// Skips the last few elements in the collection
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="count">How many elements to skip (default: 1)</param>
		/// <returns>The collection minus the last few elements</returns>
		public static IAsyncEnumerable<T> SkipLast<T>(this IAsyncEnumerable<T> data, int count = 1)
		{
			return data.Reverse().Skip(count).Reverse();
		}

		/// <summary>
		/// Splits the given collection in chunks of a given length
		/// </summary>
		/// <typeparam name="T">The type of collection</typeparam>
		/// <param name="data">The target collection</param>
		/// <param name="chunks">The max number of item per chunk</param>
		/// <returns>The split up chunks of the target collection</returns>
		public static async IAsyncEnumerable<T[]> Chunk<T>(this IAsyncEnumerable<T> data, int chunks)
		{
			var cur = new List<T>();
			int i = 0;
			await foreach (var item in data)
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
	}
}
