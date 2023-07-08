namespace CardboardBox.Discord
{
	/// <summary>
	/// An exception that is thrown when two or more commands are registered with the same name in the same scope (global or guild)
	/// </summary>
	public class DuplicateCommandException : Exception
	{
		/// <summary>
		/// The type that contains the invalid command
		/// </summary>
		public Type Target { get; }

		/// <summary>
		/// The name of the invalid command
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="target"></param>
		/// <param name="name"></param>
		public DuplicateCommandException(Type target, string name)
		{
			Target = target;
			Name = name;
		}
	}
}
