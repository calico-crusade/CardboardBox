using Discord;

namespace CardboardBox.Discord.Components;

/// <summary>
/// Represents a select menu
/// </summary>
public class SelectMenuAttribute : ComponentAttribute
{
	/// <summary>
	/// How many menu items need to be accepted at minimum
	/// </summary>
	public int MinValues { get; set; } = 1;
	
	/// <summary>
	/// The max amount of menu items that can be selected
	/// </summary>
	public int MaxValues { get; set; } = 1;

	/// <summary>
	/// A placeholder message for the selected option
	/// </summary>
	public string? Placeholder { get; set; }

	/// <summary>
	/// Whether or not the input is disabled
	/// </summary>
	public bool Disabled { get; set; } = false;

	/// <summary>
	/// Whether or not the select menu is required (only used in modals)
	/// </summary>
	public bool Required { get; set; } = true;

	/// <summary>
	/// Specifies the name of the method that should be used to populate the drop down options
	/// </summary>
	public string? OptionsMethod { get; set; }

	/// <summary>
	/// The type of select menu to provide
	/// </summary>
	public ComponentType Type { get; set; } = ComponentType.SelectMenu;

	/// <summary>
	/// The type of channels that can be selected
	/// </summary>
	public ChannelType[] ChannelTypes { get; } = Array.Empty<ChannelType>();

	/// <summary>
	/// Default constructor
	/// </summary>
	public SelectMenuAttribute() { }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="type">The type of select menu</param>
	/// <param name="channelTypes">The type of channels that can be selected.</param>
	public SelectMenuAttribute(ComponentType type, params ChannelType[] channelTypes)
	{
		Type = type;
		ChannelTypes = channelTypes;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="optionsMethod">The name of the method to fetch the options</param>
	public SelectMenuAttribute(string optionsMethod)
	{
		OptionsMethod = optionsMethod;
	}
}

/// <summary>
/// Represents a select menu option
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = true)]
public class SelectMenuOptionAttribute : Attribute
{
	/// <summary>
	/// The label for the menu option
	/// </summary>
	public string Label { get; set; }

	/// <summary>
	/// The value of the menu option
	/// </summary>
	public string Value { get; set; }

	/// <summary>
	/// The description of the menu option
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// The emote for the menu option
	/// </summary>
	public string? Emote { get; set; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="label">The text of the option</param>
	public SelectMenuOptionAttribute(string label)
	{
		Label = label;
		Value = label;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="label">The text of the option</param>
	/// <param name="value">The value to return for the option</param>
	public SelectMenuOptionAttribute(string label, string value)
	{
		Label = label;
		Value = value;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label">The text of the option</param>
    /// <param name="value">The value to return for the option</param>
    /// <param name="description">The description of the option</param>
    public SelectMenuOptionAttribute(string label, string value, string description) : this(label, value)
	{
		Description = description;
	}
}

/// <summary>
/// The default option for a select menu
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter, AllowMultiple = false)]
public class DefaultSelectMenuOptionAttribute : SelectMenuOptionAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="label">The text of the option</param>
    public DefaultSelectMenuOptionAttribute(string label) : base(label) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label">The text of the option</param>
    /// <param name="value">The value to return for the option</param>
    public DefaultSelectMenuOptionAttribute(string label, string value) : base(label, value) { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label">The text of the option</param>
    /// <param name="value">The value to return for the option</param>
    /// <param name="description">The description of the option</param>
	public DefaultSelectMenuOptionAttribute(string label, string value, string description) : base(label, value, description) { }
}
