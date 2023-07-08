namespace CardboardBox.Discord.Components;

/// <summary>
/// Represents a discord modal
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ModalAttribute : Attribute
{
    /// <summary>
    /// The title of the modal
    /// </summary>
    public string Title { get; }

    /// <summary></summary>
    /// <param name="title">The tital of the modal</param>
    public ModalAttribute(string title)
    {
        Title = title;
    }
}
