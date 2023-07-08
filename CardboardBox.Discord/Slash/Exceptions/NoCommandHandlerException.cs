namespace CardboardBox.Discord
{
	/// <summary>
	/// Exception to be thrown when a command has no registered handler
	/// </summary>
	public class NoCommandHandlerException : Exception
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="cmd"></param>
		public NoCommandHandlerException(string cmd) : base($"Slash command received, but no handlers registered: {cmd}") { }
	}
}
