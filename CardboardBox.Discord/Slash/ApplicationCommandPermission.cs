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

		/// <summary>
		/// 
		/// </summary>
		/// <param name="perms"></param>
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

		/// <summary></summary>
		public ApplicationCommandPermission() { }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		/// <param name="perm"></param>
		public ApplicationCommandPermission(ulong id, ApplicationCommandPermissionType type, bool perm)
		{
			RoleUserId = id;
			Type = type;
			Permission = perm;
		}
	}

	/// <summary>
	/// The type of application command permission
	/// </summary>
	public enum ApplicationCommandPermissionType
	{ 
		/// <summary>
		/// The permission is for a specific role
		/// </summary>
		Role = 1,
		/// <summary>
		/// The permission is for a specific user
		/// </summary>
		User = 2
	}
}
