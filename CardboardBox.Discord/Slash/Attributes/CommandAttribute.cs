namespace CardboardBox.Discord
{
	/// <summary>
	/// Represents a single global application command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class CommandAttribute : Attribute
	{
		/// <summary>
		/// The name of the command
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The description of the command
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Whether or not the command takes more than 3 seconds to respond
		/// </summary>
		public bool LongRunning { get; set; } = false;

		/// <summary>
		/// Whether or not the response should be ephemeral
		/// </summary>
		public bool Ephemeral { get; set; } = false;

		/// <summary>
		/// Default constructor for a Command
		/// </summary>
		/// <param name="name">The name of the command</param>
		/// <param name="description">The description of the command</param>
		public CommandAttribute(string name, string description)
		{
			Name = name;
			Description = description;
		}
	}

	/// <summary>
	/// Represents a single guild-specific application command
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class GuildCommandAttribute : CommandAttribute
	{
		/// <summary>
		/// The guild the application command is going to be for
		/// </summary>
		public ulong GuildId { get; }

		/// <summary>
		/// An optional collection of role ids that can use this command within the guild
		/// </summary>
		public ulong[] RoleIds { get; }

		/// <summary>
		/// Default constructor for the guild command
		/// </summary>
		/// <param name="name">The name of the command</param>
		/// <param name="description">The description of the command</param>
		/// <param name="guildId">The guild this command is for</param>
		/// <param name="roleIds">An optional collection of roles that can use this command</param>
		public GuildCommandAttribute(string name, string description, ulong guildId, params ulong[] roleIds) : base(name, description)
		{
			GuildId = guildId;
			RoleIds = roleIds;
		}
	}
}
