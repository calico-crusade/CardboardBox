namespace CardboardBox.Discord
{
	/// <summary>
	/// An exception thrown when an option is invalid
	/// </summary>
	public class InvalidOptionException : Exception
	{
		/// <summary>
		/// The type that has the invalid option
		/// </summary>
		public Type Type { get; }

		public InvalidOptionException(Type type)
		{
			Type = type;
		}
	}
}
