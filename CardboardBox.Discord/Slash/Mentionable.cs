using Discord;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Represents a mentionable option (User or Role)
	/// </summary>
	public class Mentionable
	{
		/// <summary>
		/// The role that was mentioned
		/// </summary>
		public IRole? Role { get; }

		/// <summary>
		/// The user that was mentioned
		/// </summary>
		public IUser? User { get; }

		/// <summary>
		/// Whether or not the mentioned object is a user
		/// </summary>
		public bool IsUser => User != null;

		/// <summary>
		/// Whether or not the mentioned object is a role
		/// </summary>
		public bool IsRole => Role != null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="role"></param>
		public Mentionable(IRole role)
		{
			Role = role;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="user"></param>
		public Mentionable(IUser user)
		{
			User = user;
		}
	}
}
