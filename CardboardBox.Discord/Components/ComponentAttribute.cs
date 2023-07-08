namespace CardboardBox.Discord.Components;

/// <summary>
/// Represents a default component for discord
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class ComponentAttribute : Attribute
{
	/// <summary>
	/// The row to put the component on
	/// </summary>
	public int Row { get; set; } = 0;
}
