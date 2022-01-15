using CardboardBox.Discord.Components;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Exposes helpful methods for creating a discord client using slash commands
	/// </summary>
	public interface IDiscordClient
	{
		/// <summary>
		/// Triggers the login steps for the discord client
		/// </summary>
		/// <returns>A task representing the completion of the login steps</returns>
		Task Login();

		/// <summary>
		/// Runs the given task in the background of the discord client
		/// </summary>
		/// <param name="action">The action to execute</param>
		/// <param name="cts">The <see cref="CancellationTokenSource"/> used to cancel the background task</param>
		/// <param name="delaySec">Whether or not to loop the task and how long to wait between loops</param>
		/// <returns>The instance of <see cref="IDiscordClient"/> for method chaining</returns>
		IDiscordClient Background(Func<Task> action, out CancellationTokenSource cts, double? delaySec = null);

		/// <summary>
		/// Runs the given task in the background of the discord client using the given type as the parent service
		/// </summary>
		/// <typeparam name="T">The type of the parent service to run the background task from</typeparam>
		/// <param name="action">The method reference from the parent service</param>
		/// <param name="cts">The <see cref="CancellationTokenSource"/> used to cancel the background task</param>
		/// <param name="delaySec">Whether or not to loop the task and how long to wait between loops</param>
		/// <returns>The instance of <see cref="IDiscordClient"/> for method chaining</returns>
		IDiscordClient Background<T>(Func<T, Task> action, out CancellationTokenSource cts, double? delaySec = null);
	}

	/// <summary>
	/// The concrete implemention of <see cref="IDiscordClient"/>
	/// </summary>
	public class DiscordClient : IDiscordClient
	{
		private readonly IConfiguration _config;
		private readonly IServiceProvider _services;
		private readonly ILogger _logger;
		private readonly IDiscordSlashCommandBuilderService _slash;
		private readonly ISlashReflectionService _slashRef;
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly IReactionService _reactions;
		private readonly IComponentService _buttons;
		private readonly IDiscordEventHandler _handler;

		public string Token => _config["Discord:Token"];

		public DiscordClient(
			IConfiguration config,
			IServiceProvider services,
			ILogger<DiscordClient> logger,
			IDiscordSlashCommandBuilderService slash,
			ISlashReflectionService slashRef,
			DiscordSocketClient client,
			CommandService commands,
			IReactionService reactions,
			IComponentService buttons,
			IDiscordEventHandler handler)
		{
			_client = client;
			_commands = commands;
			_config = config;
			_services = services;
			_logger = logger;
			_slash = slash;
			_slashRef = slashRef;
			_reactions = reactions;
			_buttons = buttons;
			_handler = handler;
		}

		/// <summary>
		/// Triggers the login steps for the discord client
		/// </summary>
		/// <returns>A task representing the completion of the login steps</returns>
		public async Task Login()
		{
			_client.Log += Client_Log;
			_client.Ready += Client_Ready;
			_client.MessageReceived += Client_MessageReceived;
			_client.SlashCommandExecuted += Client_SlashCommandExecuted;
			_client.ReactionAdded += Client_ReactionAdded;
			_client.ButtonExecuted += Client_ButtonExecuted;
			_client.SelectMenuExecuted += Client_SelectMenuExecuted;

			await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

			await _client.LoginAsync(TokenType.Bot, Token);
			await _client.StartAsync();
		}

		/// <summary>
		/// Handler for select menu executions
		/// </summary>
		/// <param name="arg">The select menu handler</param>
		/// <returns>A task representing the completion of the reaction handling</returns>
		public Task Client_SelectMenuExecuted(SocketMessageComponent arg)
		{
			_buttons.HandleComponent(arg);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handler for button executions
		/// </summary>
		/// <param name="arg">The button handler</param>
		/// <returns>A task representing the completion of the reaction handling</returns>
		public Task Client_ButtonExecuted(SocketMessageComponent arg)
		{
			_buttons.HandleComponent(arg);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handler for when a reaction is added to a message
		/// </summary>
		/// <param name="message">The message that was reacted to</param>
		/// <param name="channel">The channel the message was reacted in</param>
		/// <param name="reaction">The reaction that occurred</param>
		/// <returns>A task representing the completion of the reaction handling</returns>
		public Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
		{
			_reactions.ReactionAdded(message, channel, reaction);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Handler for slash commands that are executed
		/// </summary>
		/// <param name="arg">The socket slash command that was executed</param>
		/// <returns>A task representing the completion of the handler</returns>
		public Task Client_SlashCommandExecuted(SocketSlashCommand arg)
		{
			if (!HandleFluentCommand(arg))
				_slashRef.Handle(arg);
			return Task.CompletedTask;			
		}

		/// <summary>
		/// Handles fluently registered slash commands
		/// </summary>
		/// <param name="arg">The slash command that triggered the handler</param>
		/// <returns>Whether or not the command was handled</returns>
		public bool HandleFluentCommand(SocketSlashCommand arg)
		{
			try
			{
				var cmd = arg.CommandName.ToLower().Trim();
				if (_slash.Commands == null || _slash.Commands.Count <= 0)
					return false;

				if (!_slash.Commands.ContainsKey(cmd))
					return false;

				var handler = _slash.Commands[cmd];
				if (handler?.Parent == null)
				{
					_logger.LogWarning($"Parent Handler cannot be null: {arg.CommandName}");
					return false;
				}

				var parent = _services.GetService(handler.Parent);
				if (parent == null)
				{
					_logger.LogWarning($"Slash command registered, but parent object is not found: {cmd}");
					return false;
				}

				if (handler?.Method == null)
				{
					_logger.LogWarning($"Handler method cannot be null: {arg.CommandName}");
					return false;
				}

				_ = Task.Run(async () =>
				{
					try
					{
						var method = handler.Method(parent);
						await method(arg);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, $"Error occurred while executing slash command {arg.CommandName}");
					}
				});
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Error occurred while executing slash command: {arg.CommandName}");
				return false;
			}
		}

		/// <summary>
		/// Handles normally registered commands
		/// </summary>
		/// <param name="arg">The command that triggered the handler</param>
		/// <returns>A task representing the completion of the handler</returns>
		public async Task Client_MessageReceived(SocketMessage arg)
		{
			if (arg is not SocketUserMessage message) return;

			int argPos = 0;

			var param = _config["Discord:CommandParam"] ?? "!";

			if (!(message.HasStringPrefix(param, ref argPos) ||
				message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
				message.Author.IsBot)
				return;

			var context = new SocketCommandContext(_client, message);

			await _commands.ExecuteAsync(
				context: context,
				argPos: argPos,
				services: _services);
		}

		/// <summary>
		/// Handles the Client-Ready event which registered the socket slash commands
		/// </summary>
		/// <returns>A task representing the completion of the handler</returns>
		public Task Client_Ready()
		{
			_ = Task.Run(async () => await _slashRef.RegisterSlashCommands()); 

			var username = _client.CurrentUser?.Username ?? "ANON";
			_logger.LogInformation($"{username} >> Client Ready!");

			_handler.Execute(_services, _client);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Runs the given task in the background of the discord client using the given type as the parent service
		/// </summary>
		/// <typeparam name="T">The type of the parent service to run the background task from</typeparam>
		/// <param name="action">The method reference from the parent service</param>
		/// <param name="cts">The <see cref="CancellationTokenSource"/> used to cancel the background task</param>
		/// <param name="delaySec">Whether or not to loop the task and how long to wait between loops</param>
		/// <returns>The instance of <see cref="IDiscordClient"/> for method chaining</returns>
		public IDiscordClient Background<T>(Func<T, Task> action, out CancellationTokenSource cts, double? delaySec = null)
		{
			var service = _services.GetService<T>();
			if (service == null)
				throw new ArgumentNullException(nameof(service));

			return Background(() => action(service), out cts, delaySec);
		}

		/// <summary>
		/// Runs the given task in the background of the discord client
		/// </summary>
		/// <param name="action">The action to execute</param>
		/// <param name="cts">The <see cref="CancellationTokenSource"/> used to cancel the background task</param>
		/// <param name="delaySec">Whether or not to loop the task and how long to wait between loops</param>
		/// <returns>The instance of <see cref="IDiscordClient"/> for method chaining</returns>
		public IDiscordClient Background(Func<Task> action, out CancellationTokenSource cts, double? delaySec = null)
		{
			if (delaySec != null)
				return Background(async () =>
				{
					var actDelay = (int)TimeSpan.FromSeconds(delaySec ?? 0).TotalMilliseconds;
					while (true)
					{
						if (actDelay > 0)
							await Task.Delay(actDelay);
						await action();
					}
				}, out cts, null);

			cts = new CancellationTokenSource();
			var token = cts.Token;

			Task.Run(async () => await action(), token);

			return this;
		}

		/// <summary>
		/// Handles the discord clients logging handler
		/// </summary>
		/// <param name="arg">The log message</param>
		/// <returns>A task representing the completion of the handler</returns>
		public Task Client_Log(LogMessage arg)
		{
			var username = _client?.CurrentUser?.Username ?? "ANON";
			var msg = $"{username} >> {arg.Message}: {arg.Source}";
			switch (arg.Severity)
			{
				case LogSeverity.Info: _logger.LogInformation(arg.Exception, msg); break;
				case LogSeverity.Debug: _logger.LogDebug(arg.Exception, msg); break;
				case LogSeverity.Critical: _logger.LogCritical(arg.Exception, msg); break;
				case LogSeverity.Error: _logger.LogError(arg.Exception, msg); break;
				case LogSeverity.Warning: _logger.LogWarning(arg.Exception, msg); break;
				case LogSeverity.Verbose: _logger.LogTrace(arg.Exception, msg); break;
			}

			return Task.CompletedTask;
		}
	}
}
