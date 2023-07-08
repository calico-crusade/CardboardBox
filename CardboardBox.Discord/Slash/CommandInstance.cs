using Discord;
using System.Reflection;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Represents an instance of a command that has been resolved via reflection
	/// </summary>
	public class CommandInstance
	{
		/// <summary>
		/// The attribute that represents the commands configuration
		/// </summary>
		public CommandAttribute Attribute { get; set; }

		/// <summary>
		/// The parent classes type
		/// </summary>
		public Type ParentType { get; set; }

		/// <summary>
		/// The method to use as the handler
		/// </summary>
		public MethodInfo Method { get; set; }

		/// <summary>
		/// The builder that represents the configuration of the command
		/// </summary>
		public SlashCommandBuilder Builder { get; set; }

		/// <summary>
		/// A list of all of the options specified on the command
		/// </summary>
		public List<CommandOption> Options { get; set; } = new();

		/// <summary>
		/// The name of the command
		/// </summary>
		public string Name => Attribute.Name;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atr"></param>
		/// <param name="parent"></param>
		/// <param name="method"></param>
		/// <param name="bob"></param>
		public CommandInstance(CommandAttribute atr, Type parent, MethodInfo method, SlashCommandBuilder bob)
		{
			Attribute = atr;
			ParentType = parent;
			Method = method;
			Builder = bob;
		}
	}

	/// <summary>
	/// Represents an instance of a commands option
	/// </summary>
	public class CommandOption
	{
		/// <summary>
		/// The attribute that represents the options configuration
		/// </summary>
		public OptionAttribute Attribute { get; set; }

		/// <summary>
		/// The parameter the option applies to
		/// </summary>
		public ParameterInfo Parameter { get; set; }

		/// <summary>
		/// The builder that represents the configruation of the option
		/// </summary>
		public SlashCommandOptionBuilder Builder { get; set; }

		/// <summary>
		/// The index where the option appears in the methods arguments
		/// </summary>
		public int Position { get; set; }

		/// <summary>
		/// The type of the parameter
		/// </summary>
		public Type Type => Parameter.ParameterType;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="atr"></param>
		/// <param name="par"></param>
		/// <param name="bob"></param>
		/// <param name="pos"></param>
		public CommandOption(OptionAttribute atr, ParameterInfo par, SlashCommandOptionBuilder bob, int pos)
		{
			Attribute = atr;
			Parameter = par;
			Builder = bob;
			Position = pos;
		}
	}
}
