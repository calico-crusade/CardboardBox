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
		/// Optionally configures client event handlers
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithEventHandling(Action<IDiscordEventHandler> config);

		/// <summary>
		/// Optionally configures client event handlers
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithHandlers(Action<DiscordSocketClient> config);

		/// <summary>
		/// Optionally configures logging for the discord bot
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithLogging(Action<ILoggingBuilder> config);

		/// <summary>
		/// Optionally configures the <see cref="IConfiguration"/> with the existing one
		/// </summary>
		/// <param name="config">The existing <see cref="IConfiguration"/> object</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		IDiscordBotBuilder WithConfig(IConfiguration config);

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
		private readonly DiscordSocketClient _client;
		private readonly IDiscordEventHandler _handler;

		private DiscordSlashCommandBuilder? _slash;
		private IComponentHandlerService _components;

		private bool LoggingAdded = false;
		private bool ConfigAdded = false;
		private bool SlashAdded = false;

		private DiscordBotBuilder(IServiceCollection services, DiscordSocketClient client)
		{
			_services = services;
			_components = new ComponentHandlerService();
			_client = client;
			_handler = new DiscordEventHandler();
		}

		/// <summary>
		/// Optionally configures client event handlers
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithEventHandling(Action<IDiscordEventHandler> config)
		{
			config?.Invoke(_handler);
			return this;
		}


		/// <summary>
		/// Optionally configures client event handlers
		/// </summary>
		/// <param name="config">The configuration action</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithHandlers(Action<DiscordSocketClient> config)
		{
			config?.Invoke(_client);
			return this;
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
		/// Optionally configures the <see cref="IConfiguration"/> with the existing one
		/// </summary>
		/// <param name="config">The existing <see cref="IConfiguration"/> object</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public IDiscordBotBuilder WithConfig(IConfiguration config)
		{
			_services.AddSingleton(config);
			ConfigAdded = true;
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
			if (_slash == null)
			{
				_slash = new DiscordSlashCommandBuilder(_services, _components);
				_services.AddSingleton<IDiscordSlashCommandBuilderService>(_slash);
				SlashAdded = true;
			}

			config?.Invoke(_slash);
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
				.AddSingleton(_client ?? new())
				.AddSingleton(_handler)
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
		/// <param name="client">The optional discord client to use for the bot</param>
		/// <returns>The instance of <see cref="IDiscordBotBuilder"/> for method chaining</returns>
		public static IDiscordBotBuilder Start(IServiceCollection? services = null, DiscordSocketClient? client = null)
		{
			services ??= new ServiceCollection();
			client ??= new();
			return new DiscordBotBuilder(services, client);
		}
	}
}