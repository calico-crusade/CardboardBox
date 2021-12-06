namespace CardboardBox.Discord
{
	/// <summary>
	/// Exception to be thrown an a command's parent service couldn't be resolved via dependency injection
	/// </summary>
	public class NoParentServiceException : Exception
	{
		public Type Type { get; }

		public NoParentServiceException(Type type)
		{
			Type = type;
		}
	}
}
