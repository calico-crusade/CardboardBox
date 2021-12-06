using Discord;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Represents a command option
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public class OptionAttribute : Attribute
	{
		private double? _min = null;
		private double? _max = null;

		/// <summary>
		/// The name of the option (infered from parameter name if none is specified)
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// The description of the option
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// Whether or not the option is required (infered from the nullability of the type if none is specified)
		/// </summary>
		public OptBool Required { get; set; } = OptBool.NotSet;

		/// <summary>
		/// A collection of optional choices for the option
		/// </summary>
		public (string, string)[]? Choices { get; set; }

		/// <summary>
		/// The type of the option (infered from parameter type if none is specified)
		/// </summary>
		public ApplicationCommandOptionType? Type { get; set; } = null;

		/// <summary>
		/// Whether or not to use auto-complete for this option
		/// </summary>
		public OptBool AutoComplete { get; set; } = OptBool.NotSet;

		/// <summary>
		/// The minimum value this option can have.
		/// </summary>
		public double Min { get => _min ?? 0; set => _min = value; }

		/// <summary>
		/// The maximum value this option can have.
		/// </summary>
		public double Max { get => _max ?? 0; set => _max = value; }

		/// <summary>
		/// Whether or not <see cref="Min"/> is set to a value or not specified at all
		/// </summary>
		public bool IsMinSet => _min != null;

		/// <summary>
		/// Whether or not <see cref="Max"/> is set to a value or not specified at all
		/// </summary>
		public bool IsMaxSet => _max != null;

		/// <summary>
		/// Default constructor for the option
		/// </summary>
		public OptionAttribute() { }

		/// <summary>
		/// Constructor that allows specifying just the choices for the option
		/// </summary>
		/// <param name="choices">A colection of the available choices</param>
		public OptionAttribute(params (string, string)[] choices)
		{
			Choices = choices;
		}

		/// <summary>
		/// Constructor that allows specifying just the choices for the option
		/// </summary>
		/// <param name="choices">A colection of the available choices</param>
		public OptionAttribute(params string[] choices)
		{
			Choices = choices.Select(t => (t, t)).ToArray();
		}

		/// <summary>
		/// Constructor that allows for specifying the description of the option
		/// </summary>
		/// <param name="description">The description of the option</param>
		public OptionAttribute(string description)
		{
			Description = description;
		}

		/// <summary>
		/// Constructor that allows for specifying the description of the option and whether or not it's required
		/// </summary>
		/// <param name="description">The description of the option</param>
		/// <param name="required">Whether or not the option is required</param>
		public OptionAttribute(string description, bool required) : this(description)
		{
			Required = required ? OptBool.True : OptBool.False;
		}

		/// <summary>
		/// Constructor that allows for specifying the description of the option and whether or not it's required
		/// </summary>
		/// <param name="description">The description of the option</param>
		/// <param name="choices">A colection of the available choices</param>
		public OptionAttribute(string description, params string[] choices): this(choices)
		{
			Description = description;
		}

		/// <summary>
		/// Constructor that allows for specifying the description of the option and whether or not it's required
		/// </summary>
		/// <param name="description">The description of the option</param>
		/// <param name="choices">A colection of the available choices</param>
		public OptionAttribute(string description, params (string, string)[] choices) : this(choices)
		{
			Description = description;
		}

		/// <summary>
		/// Constructor that allows for specifying the description of the option and whether or not it's required
		/// </summary>
		/// <param name="description">The description of the option</param>
		/// <param name="required">Whether or not the option is required</param>
		/// <param name="choices">A colection of the available choices</param> 
		public OptionAttribute(string description, bool required, params string[] choices) : this(description, choices)
		{
			Required = required ? OptBool.True : OptBool.False;
		}

		/// <summary>
		/// Constructor that allows for specifying the description of the option and whether or not it's required
		/// </summary>
		/// <param name="description">The description of the option</param>
		/// <param name="required">Whether or not the option is required</param>
		/// <param name="choices">A colection of the available choices</param>
		public OptionAttribute(string description, bool required, params (string, string)[] choices) : this(description, choices)
		{
			Required = required ? OptBool.True : OptBool.False;
		}
	}
}
