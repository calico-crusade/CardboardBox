namespace CardboardBox
{
	public interface IVerb<TOptions> where TOptions : class
	{
		/// <summary>
		/// Executed when the command is run
		/// </summary>
		/// <param name="options">The command line argument options</param>
		/// <returns>The exit code</returns>
		Task<int> Run(TOptions options);
	}
}
