using System.Text.Json.Serialization;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Contains an array of all of the application command permissions
	/// </summary>
	public class ApplicationCommandPermissions
	{
		/// <summary>
		/// The permissions for this command
		/// </summary>
		[JsonPropertyName("permissions")]
		public ApplicationCommandPermission[] Permissions { get; set; }

		public ApplicationCommandPermissions(ApplicationCommandPermission[] perms)
		{
			Permissions = perms;
		}
	}

	/// <summary>
	/// Represents the permissions for a specific application command
	/// </summary>
	public class ApplicationCommandPermission
	{
		/// <summary>
		/// The role or user id for this permission
		/// </summary>
		[JsonPropertyName("id")]
		public ulong RoleUserId { get; set; }

		/// <summary>
		/// The type of permission (Role or User)
		/// </summary>
		[JsonPropertyName("type")]
		public ApplicationCommandPermissionType Type { get; set; }

		/// <summary>
		/// Whether or not to grant the permission to the specified user
		/// </summary>
		[JsonPropertyName("permission")]
		public bool Permission { get; set; }

		public ApplicationCommandPermission() { }

		public ApplicationCommandPermission(ulong id, ApplicationCommandPermissionType type, bool perm)
		{
			RoleUserId = id;
			Type = type;
			Permission = perm;
		}
	}

	public enum ApplicationCommandPermissionType
	{ 
		Role = 1,
		User = 2
	}
}
