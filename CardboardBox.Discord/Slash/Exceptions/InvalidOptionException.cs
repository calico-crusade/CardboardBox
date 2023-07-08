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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public InvalidOptionException(Type type)
		{
			Type = type;
		}
	}
}
