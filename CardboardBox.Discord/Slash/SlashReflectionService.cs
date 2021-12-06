using CardboardBox.Http;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CardboardBox.Discord
{
	/// <summary>
	/// Exposes methods for handling reflection based slash commands
	/// </summary>
	public interface ISlashReflectionService
	{
		/// <summary>
		/// A collection of all of the registered commands
		/// </summary>
		CommandInstance[] Commands { get; }

		/// <summary>
		/// Attempts to handle the given slash command
		/// </summary>
		/// <param name="command">The command that needs to be handled</param>
		/// <returns>Whether or not the command was handled</returns>
		bool Handle(SocketSlashCommand command);

		/// <summary>
		/// Triggers the requests to discord to register the slash commands
		/// </summary>
		/// <returns>A task representing the completion of the registration</returns>
		Task RegisterSlashCommands();
	}

	public class SlashReflectionService : ISlashReflectionService
	{
		private readonly ILogger _logger;
		private readonly IServiceProvider _services;
		private readonly IDiscordSlashCommandBuilderService _commands;
		private readonly DiscordSocketClient _client;
		private readonly IApiService _api;
		private readonly IConfiguration _config;

		/// <summary>
		/// The discord authorization token
		/// </summary>
		public string Token => _config["Discord:Token"];

		/// <summary>
		/// A collection of all of the registered commands
		/// </summary>
		public CommandInstance[] Commands => _commands.ReflectionCommands.Select(t => t.Value).ToArray();

		public SlashReflectionService(
			ILogger<SlashReflectionService> logger,
			IServiceProvider services,
			IDiscordSlashCommandBuilderService commands,
			DiscordSocketClient client,
			IConfiguration config,
			IApiService api)
		{
			_logger = logger;
			_services = services;
			_commands = commands;
			_client = client;
			_api = api;
			_config = config;
		}

		/// <summary>
		/// Attempts to handle the given slash command
		/// </summary>
		/// <param name="command">The command that needs to be handled</param>
		/// <returns>Whether or not the command was handled</returns>
		public bool Handle(SocketSlashCommand command)
		{
			var cmd = command.CommandName.ToLower().Trim();
			if (_commands == null || 
				_commands.ReflectionCommands.Count <= 0 || 
				!_commands.ReflectionCommands.ContainsKey(cmd))
				return false;

			_ = Task.Run(async () =>
			{
				try
				{
					await HandleCommand(command, cmd);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred while processing command: {0}", command.CommandName);
					await command.Modify("An error occurred! Please try again later.");
				}
			});
			return true;
		}

		/// <summary>
		/// Handles the given slash command 
		/// </summary>
		/// <param name="command">The slash command arguments</param>
		/// <param name="cmd">The handler name</param>
		/// <returns>A task that represents the completion of the handling of the command</returns>
		public async Task HandleCommand(SocketSlashCommand command, string cmd)
		{
			var handler = _commands.ReflectionCommands[cmd];
			var parent = _services.GetService(handler.ParentType);
			if (parent == null)
			{
				_logger.LogWarning("No parent service found for \"{0}\" from \"{1}\"", handler.ParentType.Name, cmd);
				return;
			}

			var args = GetParameters(command, handler);
			if (args == null) return;

			if (handler.Attribute.LongRunning)
				await command.DeferAsync();
			
			var result = handler.Method.Invoke(parent, args);
			if (result == null) return;

			var resType = result.GetType();
			if (resType == typeof(void)) return;
			if (result is not Task) return;
			await (Task)result;
		}

		/// <summary>
		/// Attempts to resolve the method parameters needed for running a slash command handler
		/// </summary>
		/// <param name="command">The slash command</param>
		/// <param name="handler">The handler instance</param>
		/// <returns>An array of the commands parameters</returns>
		public object?[]? GetParameters(SocketSlashCommand command, CommandInstance handler)
		{
			var pars = handler.Method.GetParameters();
			var args = new object?[pars.Length];

			for (var i = 0; i < args.Length; i++)
			{
				var param = pars[i];
				var act = handler.Options.FirstOrDefault(t => t.Position == i);
				if (act != null)
				{
					var option = command.Data.Options.FirstOrDefault(t => t.Name == act.Attribute.Name);
					if (option == null && (act.Attribute.Required.From() ?? false))
					{
						_logger.LogWarning($"Option required at {i} but none found by name {act.Attribute.Name}");
						return null;
					}

					args[i] = option?.Value;
					continue;
				}

				if (param.ParameterType == typeof(SocketSlashCommand))
				{
					args[i] = command;
					continue;
				}

				var serv = _services.GetService(param.ParameterType);
				if (serv != null)
				{
					args[i] = serv;
					continue;
				}

				args[i] = null;
			}

			return args;
		}

		/// <summary>
		/// Triggers the requests to discord to register the slash commands
		/// </summary>
		/// <returns>A task representing the completion of the registration</returns>
		public async Task RegisterSlashCommands()
		{
			try
			{
				await HandleGlobalCommands();

				var guildIds = _commands
					.Commands
					.Where(t => t.Value != null && t.Value.Guilds?.Length > 0)
					.SelectMany(t => t.Value?.Guilds ?? Array.Empty<ulong>())
					.Union(
						_commands
							.ReflectionCommands
							.Where(t => t.Value.Attribute is GuildCommandAttribute)
							.Select(t => ((GuildCommandAttribute)t.Value.Attribute).GuildId)
					);

				foreach (var id in guildIds)
					await HandleGuildCommands(id);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while registering slash commands.");
			}
		}

		/// <summary>
		/// Handles registering the global slash commands
		/// </summary>
		/// <returns></returns>
		public async Task HandleGlobalCommands()
		{
			var globalCommands = _commands
				.Commands
				.Where(t => t.Value != null && t.Value.Guilds?.Length == 0)
				.Select(t => (t.Value.Builder ?? new SlashCommandBuilder()).Build())
				.Concat(
					_commands
						.ReflectionCommands
						.Where(t => t.Value.Attribute is not GuildCommandAttribute)
						.Select(t => t.Value.Builder.Build())
				)
				.ToList();

			var currentGlobalCommands = await _client.GetGlobalApplicationCommandsAsync();
			foreach(var cmd in currentGlobalCommands)
			{
				var exists = globalCommands.FirstOrDefault(t => t.Name.IsSpecified && t.Name.Value == cmd.Name);
				if (exists == null)
				{
					await cmd.DeleteAsync();
					continue;
				}

				globalCommands.Remove(exists);
				var validate = Validate(exists, cmd);
				if (validate) continue;

				await _client.CreateGlobalApplicationCommandAsync(exists);
				_logger.LogInformation($"Global Application Command Update: {exists.Name}");
			}

			foreach(var cmd in globalCommands)
			{
				await _client.CreateGlobalApplicationCommandAsync(cmd);
				_logger.LogInformation($"Global Application Command Created: {cmd.Name}");
			}
		}

		/// <summary>
		/// Handles registering the guild specific slash commands
		/// </summary>
		/// <param name="guildId">The ID of the guild to register the commands for</param>
		/// <returns>A task representing the completion of the registeration</returns>
		public async Task HandleGuildCommands(ulong guildId)
		{
			var cmds = _commands
				.Commands
				.Where(t => t.Value != null && (t.Value.Guilds?.Contains(guildId) ?? false))
				.Select(t => (t.Value.Builder ?? new SlashCommandBuilder()).Build())
				.Concat(
					_commands
						.ReflectionCommands
						.Where(t => t.Value.Attribute is GuildCommandAttribute gca && gca.GuildId == guildId)
						.Select(t => t.Value.Builder.Build())
				)
				.ToList();

			var guild = _client.GetGuild(guildId);
			var currentCmds = await guild.GetApplicationCommandsAsync();
			foreach (var cmd in currentCmds)
			{
				var exists = cmds.FirstOrDefault(t => t.Name.IsSpecified && t.Name.Value == cmd.Name);
				if (exists == null)
				{
					await cmd.DeleteAsync();
					continue;
				}

				cmds.Remove(exists);
				var validate = Validate(exists, cmd);
				if (validate) continue;

				var c = await guild.CreateApplicationCommandAsync(exists);
				await HandleGuildCommandPerms(c);
				_logger.LogInformation($"Guild Application Command Update: {guild.Name} :: {exists.Name}");
			}

			foreach(var cmd in cmds)
			{
				var c = await guild.CreateApplicationCommandAsync(cmd);
				await HandleGuildCommandPerms(c);
				_logger.LogInformation($"Guild Application Command Create: {guild.Name} :: {cmd.Name}");
			}
		}

		/// <summary>
		/// Handles updating the permissions for guild specific slash commands
		/// </summary>
		/// <param name="c">The socket application command that needs their permissions updated</param>
		/// <returns>A task representing the completion of registering the guild command permissions</returns>
		public async Task HandleGuildCommandPerms(SocketApplicationCommand c)
		{
			if (!_commands.ReflectionCommands.ContainsKey(c.Name)) return;

			if (_commands.ReflectionCommands[c.Name].Attribute is not GuildCommandAttribute gcmd 
				|| gcmd.RoleIds.Length <= 0) return;

			var url = $"https://discord.com/api/v8/applications/{c.ApplicationId}/guilds/{gcmd.GuildId}/commands/{c.Id}/permissions";

			var perms = new List<ApplicationCommandPermission>();
			foreach (var roleId in gcmd.RoleIds)
				perms.Add(new(roleId, ApplicationCommandPermissionType.Role, true));

			var p = new ApplicationCommandPermissions(perms.ToArray());

			await _api.Put<object, ApplicationCommandPermissions>(url, p, c =>
			{
				c.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bot", Token);
			});
		}

		/// <summary>
		/// Checks the similarity between the already registered commands and the commands gotten from reflection
		/// </summary>
		/// <param name="a">The slash command from reflection</param>
		/// <param name="bc">The slash command registered with discord already</param>
		/// <returns>Whether or not the two commands are the same.</returns>
		public bool Validate(SlashCommandProperties a, SocketApplicationCommand bc)
		{
			var oc = a.Options.IsSpecified ? a.Options.Value.Count : 0;

			if (!ValidateOptional(a.Name, bc.Name) ||
				!ValidateOptional(a.Description, bc.Description) || 
				!ValidateOptional(a.IsDefaultPermission, bc.IsDefaultPermission) || 
				oc != bc.Options.Count)
				return false;

			if (oc == 0 && bc.Options.Count == 0) return true;

			return ValidateOptions(a.Options.Value, bc.Options.ToList());
		}

		/// <summary>
		/// Checks the similarity between the already registered options and the options gotten from reflection
		/// </summary>
		/// <param name="a">The option from reflection</param>
		/// <param name="b">The option registered with discord already</param>
		/// <returns>Whether or not the two options are the same.</returns>
		public bool ValidateOptions(List<ApplicationCommandOptionProperties> a, List<SocketApplicationCommandOption> b)
		{
			if (a.Count != b.Count) return false;

			for(var i = 0; i < a.Count; i++)
			{
				var aa = a[i];
				var bb = b[i];

				if (aa.Name != bb.Name) return false;
				if (aa.Description != bb.Description) return false;
				if (!ValidateOptions(aa.Options, bb.Options.ToList())) return false;
				if (aa.Type != bb.Type) return false;
				if (aa.MinValue != bb.MinValue) return false;
				if (aa.MaxValue != bb.MaxValue) return false;

				var ar = aa.IsRequired ?? false;
				var br = bb.IsRequired ?? false;

				if (ar != br) return false;
				if (aa.IsDefault != bb.IsDefault) return false;
				
				if ((aa.Choices?.Count ?? 0) != (bb.Choices?.Count ?? 0)) return false;

				if (aa.Choices != null && bb.Choices != null)
				{
					var bc = bb.Choices.ToArray();
					for (var c = 0; c < aa.Choices.Count; c++)
					{
						var cc = aa.Choices[c];
						var dd = bc[c];
						if (cc.Name != dd.Name) return false;
						if (cc.Value?.ToString() != dd.Value?.ToString()) return false;
					}
				}

				if ((aa.ChannelTypes?.Count ?? 0) != (bb.ChannelTypes?.Count ?? 0)) return false;

				if (bb.ChannelTypes != null && aa.ChannelTypes != null)
				{
					var dc = bb.ChannelTypes.ToArray();
					for (var d = 0; d < aa.ChannelTypes.Count; d++)
						if (aa.ChannelTypes[d] != dc[d]) return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks to see if an <see cref="Optional{T}"/> is the same as given value
		/// </summary>
		/// <typeparam name="T">The type of optional to check</typeparam>
		/// <param name="a">The optional value</param>
		/// <param name="b">The comparison value</param>
		/// <returns>Whether or not the two values are the same</returns>
		public bool ValidateOptional<T>(Optional<T> a, T b)
		{
			var comp = EqualityComparer<T>.Default;
			var val = a.IsSpecified ? a.Value : default;
			return comp.Equals(val, b);
		}
	}
}
