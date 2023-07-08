using Discord;

namespace CardboardBox.Discord.Components;

/// <summary>
/// Represents a <see cref="ButtonComponent"/>
/// </summary>
public class ButtonAttribute : ComponentAttribute
{
	/// <summary>
	/// The <see cref="Emoji"/> or <see cref="Emote"/> to use on the button
	/// </summary>
	public string? Emote { get; set; }

	/// <summary>
	/// The label for the button
	/// </summary>
	public string? Label { get; set; }

	/// <summary>
	/// The style of the button
	/// </summary>
	public ButtonStyle Style { get; set; } = ButtonStyle.Primary;

	/// <summary>
	/// The optional URL for the button
	/// </summary>
	public string? Url { get; set; }

	/// <summary>
	/// Whether or not the button is disabled
	/// </summary>
	public bool Disabled { get; set; } = false;

	/// <summary>
	/// Default constructor
	/// </summary>
	public ButtonAttribute() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label">The text shown on the button</param>
    public ButtonAttribute(string label)
	{
		Label = label;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="label">The text shown on the button</param>
	/// <param name="style">The style of the button</param>
	public ButtonAttribute(string label, ButtonStyle style): this(label)
	{
		Style = style;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="label">The text shown on the button</param>
	/// <param name="emote">The emote shown ont he button</param>
	public ButtonAttribute(string label, string emote): this(label)
	{
		Emote = emote;
	}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="label">The text shown on the button</param>
    /// <param name="emote">The emote shown ont he button</param>
	/// <param name="style">The style of the button</param>
    public ButtonAttribute(string label, string emote, ButtonStyle style): this(label, emote)
	{
		Style = style;
	}
}
