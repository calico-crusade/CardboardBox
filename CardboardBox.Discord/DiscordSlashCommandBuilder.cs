using CardboardBox.Discord.Components;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CardboardBox.Discord;

/// <summary>
/// Exposes methods used for registering slash commands
/// </summary>
public interface IDiscordSlashCommandBuilder
{
	/// <summary>
	/// Registers a fluent slash command
	/// </summary>
	/// <typeparam name="T">The type of the parent class</typeparam>
	/// <param name="command">The name of the command</param>
	/// <param name="target">The resolver for the method to use for the command</param>
	/// <param name="builder">The builder for the command, used to configure the command with discord</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	IDiscordSlashCommandBuilder With<T>(string command, Func<T, Func<SocketSlashCommand, Task>> target, Action<SlashCommandBuilder>? builder = null);

	/// <summary>
	/// Registers a fluent slash command for specific guilds
	/// </summary>
	/// <typeparam name="T">The type of the parent class</typeparam>
	/// <param name="command">The name of the command</param>
	/// <param name="guilds">The guilds the slash commands can be used in</param>
	/// <param name="target">The resolver for the method to use for the command</param>
	/// <param name="builder">The builder for the command, used to configure the command with discord</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	IDiscordSlashCommandBuilder WithGuild<T>(string command, ulong[] guilds, Func<T, Func<SocketSlashCommand, Task>> target, Action<SlashCommandBuilder>? builder = null);

	/// <summary>
	/// Registers attribute based commands for the given class
	/// </summary>
	/// <typeparam name="T">The type of class to register the commands for</typeparam>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	IDiscordSlashCommandBuilder With<T>();

	/// <summary>
	/// Registers attribute based commands for the given type
	/// </summary>
	/// <param name="type">The type of class to register the commands for</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	IDiscordSlashCommandBuilder With(Type type);

	/// <summary>
	/// Registers the given message component type with the service collection
	/// </summary>
	/// <typeparam name="T">The type representing the component handler</typeparam>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	IDiscordSlashCommandBuilder WithComponent<T>() where T : ComponentHandler;

	/// <summary>
	/// Registers the given message component type with the service collection
	/// </summary>
	/// <param name="type">The type representing the component handler</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	IDiscordSlashCommandBuilder WithComponent(Type type);
}

/// <summary>
/// Exposes the commands that have been registered for the handlers
/// </summary>
public interface IDiscordSlashCommandBuilderService : IDiscordSlashCommandBuilder
{
	/// <summary>
	/// All of the fluent slash command instances
	/// </summary>
	Dictionary<string, SlashInstance> Commands { get; }

	/// <summary>
	/// All of the reflection based slash command instances
	/// </summary>
	Dictionary<string, CommandInstance> ReflectionCommands { get; }
}

/// <summary>
/// Exposes methods used for registering slash commands
/// </summary>
public class DiscordSlashCommandBuilder : IDiscordSlashCommandBuilderService
{
	private readonly IServiceCollection _services;
	private readonly IComponentHandlerService _components;

	private readonly List<Type> _registeredTypes = new();

	/// <summary>
	/// 
	/// </summary>
	/// <param name="services"></param>
	/// <param name="components"></param>
	public DiscordSlashCommandBuilder(
		IServiceCollection services,
		IComponentHandlerService components)
	{
		_services = services;
		_components = components;
	}

	/// <summary>
	/// All of the fluent slash command instances
	/// </summary>
	public Dictionary<string, SlashInstance> Commands { get; } = new();

	/// <summary>
	/// All of the reflection based slash command instances
	/// </summary>
	public Dictionary<string, CommandInstance> ReflectionCommands { get; } = new();

	/// <summary>
	/// Registers a fluent slash command for specific guilds
	/// </summary>
	/// <typeparam name="T">The type of the parent class</typeparam>
	/// <param name="command">The name of the command</param>
	/// <param name="guilds">The guilds the slash commands can be used in</param>
	/// <param name="target">The resolver for the method to use for the command</param>
	/// <param name="builder">The builder for the command, used to configure the command with discord</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	public IDiscordSlashCommandBuilder WithGuild<T>(string command, ulong[] guilds, Func<T, Func<SocketSlashCommand, Task>> target, Action<SlashCommandBuilder>? builder = null)
	{
		RegisterType(typeof(T));
		command = command.ToLower().Trim();

		var bob = new SlashCommandBuilder()
			.WithName(command);

		builder?.Invoke(bob);

		var instance = new SlashInstance
		{
			Command = command,
			Builder = bob,
			Parent = typeof(T),
			Method = (o) => target((T)o),
			Guilds = guilds
		};

		if (Commands.ContainsKey(command))
			throw new Exception($"Slash command already exists with name: \"{command}\"");

		Commands.Add(command, instance);

		return this;
	}

	/// <summary>
	/// Registers a fluent slash command
	/// </summary>
	/// <typeparam name="T">The type of the parent class</typeparam>
	/// <param name="command">The name of the command</param>
	/// <param name="target">The resolver for the method to use for the command</param>
	/// <param name="builder">The builder for the command, used to configure the command with discord</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	public IDiscordSlashCommandBuilder With<T>(string command, Func<T, Func<SocketSlashCommand, Task>> target, Action<SlashCommandBuilder>? builder = null)
	{
		RegisterType(typeof(T));
		command = command.ToLower().Trim();

		var bob = new SlashCommandBuilder()
			.WithName(command);

		builder?.Invoke(bob);

		var instance = new SlashInstance
		{
			Command = command,
			Builder = bob,
			Parent = typeof(T),
			Method = (o) => target((T)o)
		};

		if (Commands.ContainsKey(command))
			throw new Exception($"Slash command already exists with name: \"{command}\"");

		Commands.Add(command, instance);

		return this;
	}

	/// <summary>
	/// Registers attribute based commands for the given type
	/// </summary>
	/// <param name="type">The type of class to register the commands for</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	public IDiscordSlashCommandBuilder With(Type type)
	{
		RegisterType(type);

		foreach (var command in DetermineCommands(type))
		{
			if (ReflectionCommands.ContainsKey(command.Name))
				throw new DuplicateCommandException(type, command.Name);

			ReflectionCommands.Add(command.Name, command);
		}

		return this;
	}

	/// <summary>
	/// Registers attribute based commands for the given class
	/// </summary>
	/// <typeparam name="T">The type of class to register the commands for</typeparam>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	public IDiscordSlashCommandBuilder With<T>() => With(typeof(T));

	/// <summary>
	/// Registers the given message component type with the service collection
	/// </summary>
	/// <typeparam name="T">The type representing the component handler</typeparam>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	public IDiscordSlashCommandBuilder WithComponent<T>() where T: ComponentHandler => WithComponent(typeof(T));

	/// <summary>
	/// Registers the given message component type with the service collection
	/// </summary>
	/// <param name="type">The type representing the component handler</param>
	/// <returns>The instance of <see cref="IDiscordSlashCommandBuilder"/> for method chaining</returns>
	public IDiscordSlashCommandBuilder WithComponent(Type type)
	{
		RegisterType(type);
		return this;
	}

	/// <summary>
	/// Registers the given type with the dependency injection service collection
	/// </summary>
	/// <param name="type">The type to register</param>
	public void RegisterType(Type type)
	{
		if (_registeredTypes.Contains(type))
			return;

        _components.RegisterHandlers(type);
        _registeredTypes.Add(type);
		_services.AddTransient(type);
	}

	/// <summary>
	/// Gets a list of all of the command instances from the given type
	/// </summary>
	/// <typeparam name="T">The type to determine commands from</typeparam>
	/// <returns>A collection of all of the command instances</returns>
	public static IEnumerable<CommandInstance> DetermineCommands<T>() => DetermineCommands(typeof(T));

	/// <summary>
	/// Gets a list of all of the command instances from the given type
	/// </summary>
	/// <param name="type">The type to determine commands from</param>
	/// <returns>A collection of all of the command instances</returns>
	public static IEnumerable<CommandInstance> DetermineCommands(Type type)
	{
		var methods = type.GetMethods();

		foreach (var method in methods)
		{
			var cmdAtr = method.GetCustomAttribute<CommandAttribute>();
			if (cmdAtr == null) continue;

			var bob = new SlashCommandBuilder()
				.WithName(cmdAtr.Name)
				.WithDescription(cmdAtr.Description);

			if (cmdAtr is GuildCommandAttribute gcmd && gcmd.RoleIds.Length > 0)
				bob.WithDefaultPermission(false);

			var cmd = new CommandInstance(cmdAtr, type, method, bob);

			var pars = method.GetParameters();
			for (var i = 0; i < pars.Length; i++)
			{
				var par = pars[i];

				var optAtr = par.GetCustomAttribute<OptionAttribute>();
				if (optAtr == null) continue;

				var obob = Builder(par, optAtr);
				cmd.Options.Add(new(optAtr, par, obob, i));
				bob.AddOption(obob);
			}

			yield return cmd;
		}
	}

	/// <summary>
	/// Creates a <see cref="SlashCommandOptionBuilder"/> from the given reflection data
	/// </summary>
	/// <param name="par">The parameter the option is for</param>
	/// <param name="optAtr">The option attribute</param>
	/// <returns>The builder for the given reflection data</returns>
	/// <exception cref="ArgumentNullException">Throws when the option's name is not resolvable</exception>
	public static SlashCommandOptionBuilder Builder(ParameterInfo par, OptionAttribute optAtr)
	{
		optAtr.Name = (optAtr.Name ?? par.Name ?? "").ToLower().Trim();
		if (string.IsNullOrEmpty(optAtr.Name))
			throw new ArgumentNullException(nameof(optAtr));

		var optBob = new SlashCommandOptionBuilder()
			.WithName(optAtr.Name);

		if (!string.IsNullOrEmpty(optAtr.Description)) optBob.WithDescription(optAtr.Description);
		if (optAtr.IsMaxSet) optBob.WithMaxValue(optAtr.Max);
		if (optAtr.IsMinSet) optBob.WithMinValue(optAtr.Min);
		if (optAtr.AutoComplete != OptBool.NotSet) optBob.WithAutocomplete(optAtr.AutoComplete.From() ?? false);

		if (optAtr.Choices != null && optAtr.Choices.Length > 0)
			foreach (var (key, value) in optAtr.Choices)
				optBob.AddChoice(key, value);

		var (trueType, nullable) = DetermineType(par, optAtr);
		optAtr.Type = trueType;
		optBob.WithType(trueType);

		if (optAtr.Required == OptBool.NotSet)
			optAtr.Required = (!nullable).From();

		optBob.WithRequired(optAtr.Required.From() ?? false);
		return optBob;
	}

    /// <summary>
    /// Determines the appropriate <see cref="ApplicationCommandOptionType"/> from the given parameter
    /// </summary>
    /// <param name="param">The parameter the option is for</param>
    /// <param name="opt">The option attribute</param>
    /// <returns>The infered <see cref="ApplicationCommandOptionType"/> and whether or not it's nullable</returns>
    /// <exception cref="InvalidOptionException">Throws if the given parameter type cannot be resolved.</exception>
    public static (ApplicationCommandOptionType, bool?) DetermineType(ParameterInfo param, OptionAttribute opt)
	{
		if (opt.Type != null)
			return (opt.Type.Value, opt.Required.From());

		return DetermineType(param);
	}

    /// <summary>
    /// Detereimines the appropriate <see cref="ApplicationCommandOptionType"/> from the given parameter
    /// </summary>
    /// <param name="param">The parameter the option is for</param>
    /// <returns>The infered <see cref="ApplicationCommandOptionType"/> and whether or not it's nullable</returns>
    /// <exception cref="InvalidOptionException">Throws if the given parameter type cannot be resolved.</exception>
    public static (ApplicationCommandOptionType, bool?) DetermineType(ParameterInfo param)
	{
        var (nullable, trueType) = IsNullable(param.ParameterType);

        if (trueType == typeof(string)) return (ApplicationCommandOptionType.String, nullable);
        if (trueType == typeof(double)) return (ApplicationCommandOptionType.Number, nullable);
        if (trueType == typeof(int) || trueType == typeof(long)) return (ApplicationCommandOptionType.Integer, nullable);
        if (trueType == typeof(bool)) return (ApplicationCommandOptionType.Boolean, nullable);

        if (typeof(IUser).IsAssignableFrom(trueType)) return (ApplicationCommandOptionType.User, nullable);
        if (typeof(IRole).IsAssignableFrom(trueType)) return (ApplicationCommandOptionType.Role, nullable);
        if (typeof(IChannel).IsAssignableFrom(trueType)) return (ApplicationCommandOptionType.Channel, nullable);
        if (trueType == typeof(Mentionable)) return (ApplicationCommandOptionType.Mentionable, nullable);

        throw new InvalidOptionException(trueType);
    }

    /// <summary>
    /// Determines if the given type is nullable
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>Whether or not the type is nullable as well as the underlying nullable type</returns>
    public static (bool, Type) IsNullable(Type type)
	{
		if (!type.IsValueType) return (true, type);

		var nullType = Nullable.GetUnderlyingType(type);
		if (nullType != null) return (true, nullType);

		return (false, type);
	}
}
