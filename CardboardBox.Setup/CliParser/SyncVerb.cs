namespace CardboardBox
{
	/// <summary>
	/// Represents a synchronously executed command verb
	/// </summary>
	/// <typeparam name="TOptions"></typeparam>
	public abstract class SyncVerb<TOptions> : IVerb<TOptions> where TOptions : class
	{
		/// <summary>
		/// Executed when the command is run
		/// </summary>
		/// <param name="options">The command line argument options</param>
		/// <returns>The exit code</returns>
		public virtual Task<int> Run(TOptions options)
		{
			var results = RunSync(options);
			return Task.FromResult(results);
		}

		/// <summary>
		/// Executed when the command is run
		/// </summary>
		/// <param name="options">The command line argument options</param>
		/// <returns>The exit code</returns>
		public abstract int RunSync(TOptions options);
	}
}
