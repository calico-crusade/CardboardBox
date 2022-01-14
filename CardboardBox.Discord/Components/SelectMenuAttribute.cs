namespace CardboardBox.Discord.Components
{
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
		/// Specifies the name of the method that should be used to populate the drop down options
		/// </summary>
		public string? OptionsMethod { get; set; }

		public SelectMenuAttribute() { }

		public SelectMenuAttribute(string optionsMethod)
		{
			OptionsMethod = optionsMethod;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
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

		public SelectMenuOptionAttribute(string label)
		{
			Label = label;
			Value = label;
		}

		public SelectMenuOptionAttribute(string label, string value)
		{
			Label = label;
			Value = value;
		}

		public SelectMenuOptionAttribute(string label, string value, string description) : this(label, value)
		{
			Description = description;
		}
	}

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class DefaultSelectMenuOptionAttribute : SelectMenuOptionAttribute
	{
		public DefaultSelectMenuOptionAttribute(string label) : base(label) { }
		public DefaultSelectMenuOptionAttribute(string label, string value) : base(label, value) { }
		public DefaultSelectMenuOptionAttribute(string label, string value, string description) : base(label, value, description) { }
	}
}
