using Discord;
using Discord.WebSocket;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Represents and instance of a fluent slash command
	/// </summary>
	public class SlashInstance
	{
		/// <summary>
		/// The optional guild Ids for this slash command
		/// </summary>
		public ulong[]? Guilds { get; set; }

		/// <summary>
		/// The command name
		/// </summary>
		public string? Command { get; set; }

		/// <summary>
		/// The slash command builder
		/// </summary>
		public SlashCommandBuilder? Builder { get; set; }

		/// <summary>
		/// The type of the parent class
		/// </summary>
		public Type? Parent { get; set; }

		/// <summary>
		/// The method resolver for this command
		/// </summary>
		public Func<object, Func<SocketSlashCommand, Task>>? Method { get; set; }
	}
}
