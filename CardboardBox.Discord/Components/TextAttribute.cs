using Discord;

namespace CardboardBox.Discord.Components;

/// <summary>
/// Represents a text input element on a modal
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class TextAttribute : Attribute
{
    /// <summary>
    /// The label for the input box
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// The style of input box
    /// </summary>
    public TextInputStyle Style { get; set; }

    /// <summary>
	/// The minimum amount of characters that can be entered
	/// </summary>
	public int? MinLength { get; set; }

    /// <summary>
    /// The maximum amount of characters that can be entered
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// The placeholder text for the text input
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    /// Whether or not this input is required
    /// </summary>
    public bool? Required { get; set; } = null;

    /// <summary>
    /// The default value of the text input
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// The row to put the component onto
    /// </summary>
    public int Row { get; set; } = 0;

    /// <summary></summary>
    /// <param name="label">The label for the input box</param>
    /// <param name="placeholder">The placeholder text for the text input</param>
    /// <param name="style">The style of input box</param>
    public TextAttribute(string label, string? placeholder = null, TextInputStyle style = TextInputStyle.Short)
    {
        Label = label;
        Placeholder = placeholder;
        Style = style;
    }
}
