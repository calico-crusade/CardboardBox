using CardboardBox.Discord.Components;
using CardboardBox.Http;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Exposes methods for building a discord bot with slash commands
	/// </summary>
	public interface IDiscordBotBuilder
	{
		/// <summary>
		/// Optionally configures logging for the discord bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithLogging(Action<ILoggingBuilder> config);

		/// <summary>
		/// Optionally configures an <see cref="IConfiguration"/> instance from the given json file path
		/// </summary>
		/// <param name="file">The file path for the json file</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithConfig(string file);

		/// <summary>
		/// Optionally configures any dependency injection references necessary for the bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithServices(Action<IServiceCollection> config);

		/// <summary>
		/// Optionally configures any Slash commands necessary for the bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithSlashCommands(Action<IDiscordSlashCommandBuilder> config);

		/// <summary>
		/// Builds an instance of the <see cref="IDiscordClient"/> using dependency injection and the given configurations
		/// </summary>
		/// <returns>The instance of the <see cref="IDiscordClient"/></returns>
		IDiscordClient Build();
	}

	public class DiscordBotBuilder : IDiscordBotBuilder
	{
		private readonly IServiceCollection _services;

		private DiscordSlashCommandBuilder? _slash;
		private IComponentHandlerService _components;

		private bool LoggingAdded = false;
		private bool ConfigAdded = false;
		private bool SlashAdded = false;

		private DiscordBotBuilder(IServiceCollection services)
		{
			_services = services;
			_components = new ComponentHandlerService();
		}

		/// <summary>
		/// Optionally configures logging for the discord bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithLogging(Action<ILoggingBuilder>? config)
		{
			_services.AddLogging(c => config?.Invoke(c));
			LoggingAdded = true;
			return this;
		}

		/// <summary>
		/// Optionally configures an <see cref="IConfiguration"/> instance from the given json file path
		/// </summary>
		/// <param name="file">The file path for the json file</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithConfig(string file)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile(file, false, true)
				.AddEnvironmentVariables()
				.Build();

			_services.AddSingleton<IConfiguration>(config);
			ConfigAdded = true;
			return this;
		}

		/// <summary>
		/// Optionally configures any dependency injection references necessary for the bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithServices(Action<IServiceCollection>? config)
		{
			config?.Invoke(_services);
			return this;
		}

		/// <summary>
		/// Optionally configures any Slash commands necessary for the bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithSlashCommands(Action<IDiscordSlashCommandBuilder>? config)
		{
			if (SlashAdded) throw new Exception("Slash commands have already been added. Please only register them once.");

			_slash = new DiscordSlashCommandBuilder(_services, _components);
			config?.Invoke(_slash);

			_services.AddSingleton<IDiscordSlashCommandBuilderService>(_slash);
			SlashAdded = true;
			return this;
		}

		/// <summary>
		/// Builds an instance of the <see cref="IDiscordClient"/> using dependency injection and the given configurations
		/// </summary>
		/// <returns>The instance of the <see cref="IDiscordClient"/></returns>
		public IDiscordClient Build()
		{
			if (!ConfigAdded)
				WithConfig("appsettings.json");

			if (!SlashAdded)
				WithSlashCommands(null);

			if (!LoggingAdded)
				WithLogging(c =>
					c.AddSerilog(new LoggerConfiguration()
					.MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Error)
					.MinimumLevel.Override("Microsoft.Extensions.Http.DefaultHttpClientFactory", Serilog.Events.LogEventLevel.Error)
					.WriteTo.Console()
					.WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Day)
					.MinimumLevel.Debug()
					.CreateLogger()));

			_services
				.AddCardboardHttp()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<IDiscordClient, DiscordClient>()
				.AddSingleton<ISlashReflectionService, SlashReflectionService>()
				.AddSingleton<IReactionService, ReactionService>()
				.AddSingleton<IComponentService, ComponentService>()
				.AddSingleton(_components);

			foreach (var (_, instance) in _slash?.Commands ?? new())
			{
				if (instance?.Parent != null)
					_services.AddTransient(instance.Parent);
			}

			var provider = _services.BuildServiceProvider();
			_services.AddSingleton<IServiceProvider>(provider);

			return provider.GetRequiredService<IDiscordClient>();
		}

		/// <summary>
		/// Starts the process of building a discord bot
		/// </summary>
		/// <param name="services">The optional service collection to use for the bot</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public static IDiscordBotBuilder Start(IServiceCollection? services = null)
		{
			services ??= new ServiceCollection();
			return new DiscordBotBuilder(services);
		}
	}
}