namespace CardboardBox
{
	public static class TaskExtensions
	{
		/// <summary>
		/// Chainable <see cref="Task.WhenAll(Task[])"/>
		/// </summary>
		/// <typeparam name="T">The return result of the tasks</typeparam>
		/// <param name="tasks">The collection of tasks to await</param>
		/// <returns>The awaited tasks</returns>
		public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
		{
			return Task.WhenAll(tasks);
		}

		/// <summary>
		/// Chainable <see cref="Task.WhenAll(Task[])"/>
		/// </summary>
		/// <param name="tasks">The collection of tasks to await</param>
		/// <returns>The awaited tasks</returns>
		public static Task WhenAll(this IEnumerable<Task> tasks)
		{
			return Task.WhenAll(tasks);
		}
	}
}
