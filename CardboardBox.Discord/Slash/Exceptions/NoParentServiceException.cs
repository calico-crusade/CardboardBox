namespace CardboardBox.Discord
{
	/// <summary>
	/// Exception to be thrown an a command's parent service couldn't be resolved via dependency injection
	/// </summary>
	public class NoParentServiceException : Exception
	{
		/// <summary>
		/// The type of parent service
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		public NoParentServiceException(Type type)
		{
			Type = type;
		}
	}
}
