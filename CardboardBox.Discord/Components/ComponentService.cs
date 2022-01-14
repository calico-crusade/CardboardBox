using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace CardboardBox.Discord.Components
{
	public interface IComponentService
	{
		/// <summary>
		/// Generates the message components for a message that has yet to be sent
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interactions</typeparam>
		/// <param name="channel">The channel the interaction will happen in</param>
		/// <returns>The message components to be sent to discord</returns>
		Task<MessageComponent> Components<T>(IChannel channel) where T : ComponentHandler;

		/// <summary>
		/// Generates the message components for the given message interaction
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interaction</typeparam>
		/// <param name="msg">The message the interaction is being triggered from</param>
		/// <returns>The message components to be sent to discord</returns>
		Task<MessageComponent> Components<T>(IMessage msg) where T : ComponentHandler;

		/// <summary>
		/// Generates the message componets for the given Application command interaction
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interaction</typeparam>
		/// <param name="cmd">The application command interaction</param>
		/// <returns>The message components to be sent to discord</returns>
		Task<MessageComponent> Components<T>(SocketSlashCommand cmd) where T : ComponentHandler;

		/// <summary>
		/// Generates the message components for the given interaction
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interactions</typeparam>
		/// <param name="channel">The channel the interaction will happen in</param>
		/// <param name="user">The user that triggered the interaction</param>
		/// <param name="message">The message that triggered the interaction</param>
		/// <returns>The message components to be sent to discord</returns>
		Task<MessageComponent> Components<T>(IChannel channel, IUser user, IMessage? message = null) where T : ComponentHandler;

		/// <summary>
		/// Generates the message components for the given interaction
		/// </summary>
		/// <param name="type">The class type that handles the component interactions</param>
		/// <param name="channel">The channel the interaction will happen in</param>
		/// <param name="user">The user that triggered the interaction</param>
		/// <param name="message">The message that triggered the interaction</param>
		/// <returns>The message components to be sent to discord</returns>
		Task<MessageComponent> Components(Type type, IChannel channel, IUser user, IMessage? message);

		/// <summary>
		/// Handles the given socket component interaction
		/// </summary>
		/// <param name="component">The interaction that occurred</param>
		void HandleComponent(SocketMessageComponent component);
	}

	public class ComponentService : IComponentService
	{
		private readonly ILogger _logger;
		private readonly DiscordSocketClient _client;
		private readonly IServiceProvider _provider;
		private readonly IComponentHandlerService _handlers;

		public ComponentService(
			ILogger<ComponentService> logger,
			DiscordSocketClient client,
			IServiceProvider provider,
			IComponentHandlerService handlers)
		{
			_logger = logger;
			_client = client;
			_provider = provider;
			_handlers = handlers;
		}

		/// <summary>
		/// Generates the message components for a message that has yet to be sent
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interactions</typeparam>
		/// <param name="channel">The channel the interaction will happen in</param>
		/// <returns>The message components to be sent to discord</returns>
		public Task<MessageComponent> Components<T>(IChannel channel) where T: ComponentHandler => Components<T>(channel, _client.CurrentUser);

		/// <summary>
		/// Generates the message components for the given message interaction
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interaction</typeparam>
		/// <param name="msg">The message the interaction is being triggered from</param>
		/// <returns>The message components to be sent to discord</returns>
		public Task<MessageComponent> Components<T>(IMessage msg) where T : ComponentHandler => Components<T>(msg.Channel, msg.Author, msg);

		/// <summary>
		/// Generates the message componets for the given Application command interaction
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interaction</typeparam>
		/// <param name="cmd">The application command interaction</param>
		/// <returns>The message components to be sent to discord</returns>
		public Task<MessageComponent> Components<T>(SocketSlashCommand cmd) where T : ComponentHandler => Components<T>(cmd.Channel, cmd.User);

		/// <summary>
		/// Generates the message components for the given interaction
		/// </summary>
		/// <typeparam name="T">The class type that handles the component interactions</typeparam>
		/// <param name="channel">The channel the interaction will happen in</param>
		/// <param name="user">The user that triggered the interaction</param>
		/// <param name="message">The message that triggered the interaction</param>
		/// <returns>The message components to be sent to discord</returns>
		public Task<MessageComponent> Components<T>(IChannel channel, IUser user, IMessage? message = null) where T : ComponentHandler => Components(typeof(T), channel, user, message);

		/// <summary>
		/// Generates the message components for the given interaction
		/// </summary>
		/// <param name="type">The class type that handles the component interactions</param>
		/// <param name="channel">The channel the interaction will happen in</param>
		/// <param name="user">The user that triggered the interaction</param>
		/// <param name="message">The message that triggered the interaction</param>
		/// <returns>The message components to be sent to discord</returns>
		public async Task<MessageComponent> Components(Type type, IChannel channel, IUser user, IMessage? message)
		{
			if (!typeof(ComponentHandler).IsAssignableFrom(type))
				throw new ArgumentException($"Type does not implement `{nameof(ComponentHandler)}`", type.FullName);

			var builder = new ComponentBuilder();
			var methods = type.GetMethods();
			foreach (var method in methods)
			{
				var compAtr = method.GetCustomAttribute<ComponentAttribute>();
				if (compAtr == null) continue;

				if (compAtr is ButtonAttribute btn)
				{
					HandleButton(btn, method, builder);
					continue;
				}

				if (compAtr is not SelectMenuAttribute sm)
				{
					_logger.LogWarning($"Unknown `{nameof(ComponentAttribute)}`: {compAtr.GetType().Name}");
					continue;
				}

				await HandleSelectMenu(sm, method, type, builder, channel, user, message);
			}

			return builder.Build();
		}

		/// <summary>
		/// Handles adding <see cref="SelectMenuComponent"/> to the given interaction
		/// </summary>
		/// <param name="sm">The <see cref="SelectMenuAttribute"/> that represents the select menu handler</param>
		/// <param name="method">The method that will be triggered when handling the interaction</param>
		/// <param name="type">The type of the class that will handle the interaction</param>
		/// <param name="builder">The <see cref="ComponentBuilder"/></param>
		/// <param name="channel">The channel the interaction happened in</param>
		/// <param name="user">The user that triggered the interaction</param>
		/// <param name="message">The message that triggered the interaction</param>
		/// <returns>A task representing the completion of the request</returns>
		/// <exception cref="ArgumentException">Thrown when the min or max values field on the select menu are less than 1</exception>
		public async Task HandleSelectMenu(SelectMenuAttribute sm, MethodInfo method, Type type, ComponentBuilder builder, IChannel channel, IUser user, IMessage? message)
		{
			if (sm.MinValues < 1 || sm.MaxValues < 1)
				throw new ArgumentException("Min/Max values for Select Menu must be 1 or greater.", "(Max/Min)Values");

			var id = _handlers.IdFromMethod(method);
			var options = new List<SelectMenuOptionBuilder>();

			if (!string.IsNullOrEmpty(sm.OptionsMethod))
				options.AddRange(await MenuOptionsFromMethod(sm.OptionsMethod, type, channel, user, message));

			options.AddRange(MenuOptionsFromAttributes(method));

			builder.WithSelectMenu(id, options, sm.Placeholder, sm.MinValues, sm.MaxValues, sm.Disabled, sm.Row);
		}

		/// <summary>
		/// Gets all of the <see cref="SelectMenuOptionAttribute"/> attributes from the given method
		/// </summary>
		/// <param name="method">The method to get the attributes from</param>
		/// <returns>A collection of all of the select menu options</returns>
		public SelectMenuOptionBuilder[] MenuOptionsFromAttributes(MethodInfo method)
		{
			var atrs = method.GetCustomAttributes<SelectMenuOptionAttribute>()?.ToArray();
			if (atrs == null || atrs.Length == 0) return Array.Empty<SelectMenuOptionBuilder>();

			return atrs.Select(t =>
			{
				var builder = new SelectMenuOptionBuilder()
					.WithLabel(t.Label)
					.WithValue(t.Value);

				if (!string.IsNullOrEmpty(t.Description))
					builder.WithValue(t.Description);

				if (!string.IsNullOrEmpty(t.Emote))
					builder.WithEmote(t.Emote.GetEmote());

				if (t is DefaultSelectMenuOptionAttribute)
					builder.WithDefault(true);

				return builder;
			}).ToArray();
		}

		/// <summary>
		/// Gets all of the <see cref="SelectMenuOptionBuilder[]"/> for the given method
		/// </summary>
		/// <param name="method">The method to get the select menu option builders from</param>
		/// <param name="type">The type of the class that will handle the interaction</param>
		/// <param name="channel">The channel the interaction happened in</param>
		/// <param name="user">The user that triggered the interaction</param>
		/// <param name="message">The message that triggered the interaction</param>
		/// <returns>A task representing the completion of the request</returns>
		/// <exception cref="ArgumentException">Thrown if the method is invalid</exception>
		/// <exception cref="NullReferenceException">Thrown if the service type is invalid</exception>
		public async Task<SelectMenuOptionBuilder[]> MenuOptionsFromMethod(string method, Type type, IChannel channel, IUser user, IMessage? message)
		{
			var minfo = type.GetMethod(method);
			if (minfo == null) 
				throw new ArgumentException($"Cannot find method `{method}` in `{type.Name}` type.", nameof(method));

			var instance = _provider.GetService(type);
			if (instance == null)
				throw new NullReferenceException($"Cannot get instance of type `{type.Name}`");

			if (minfo.ReturnType != typeof(Task<SelectMenuOptionBuilder[]>))
				throw new ArgumentException($"Method `{method}` on type `{type.Name}` does not return `Task<SelectMenuOptionBuilder[]>`");

			((ComponentHandler)instance).SetContext(channel, user, _client, message);

			var pars = Parameters(minfo, user, channel, message);
			var res = minfo.Invoke(instance, pars);
			if (res == null)
				throw new ArgumentException($"Results of `{method}` on type `{type.Name}` returned null.");

			var returnType = (Task<SelectMenuOptionBuilder[]>)res;
			return await returnType;
		}

		/// <summary>
		/// Ensures the parameters for executing stray methods are valid
		/// </summary>
		/// <param name="info">The method info</param>
		/// <param name="user">The user executing the method</param>
		/// <param name="channel">The channel the method is executing for</param>
		/// <param name="message">The message the method is executing for</param>
		/// <returns>An array of all of the parameters for the method</returns>
		public object?[] Parameters(MethodInfo info, IUser user, IChannel channel, IMessage? message)
		{
			var paras = info.GetParameters();
			if (paras.Length == 0) return Array.Empty<object>();

			var outPars = new object?[paras.Length];
			for(var i = 0; i < paras.Length; i++)
			{
				var par = paras[i];
				if (typeof(IUser).IsAssignableFrom(par.ParameterType))
				{
					outPars[i] = user;
					continue;
				}

				if (typeof(IChannel).IsAssignableFrom(par.ParameterType))
				{
					outPars[i] = channel;
					continue;
				}


				if (typeof(IMessage).IsAssignableFrom(par.ParameterType))
				{
					outPars[i] = message;
					continue;
				}

				var inter = _provider.GetService(par.ParameterType);
				outPars[i] = inter;
			}

			return outPars;
		}

		/// <summary>
		/// Handles adding a button to the component builder
		/// </summary>
		/// <param name="btn">The attribute that represents the buttons config</param>
		/// <param name="method">The method the attribute handles</param>
		/// <param name="builder">The component builder</param>
		public void HandleButton(ButtonAttribute btn, MethodInfo method, ComponentBuilder builder)
		{
			var id = _handlers.IdFromMethod(method);
			builder.WithButton(btn.Label, id, btn.Style, btn.Emote?.GetEmote(), btn.Url, btn.Disabled, btn.Row);
		}

		/// <summary>
		/// Handles the given socket component interaction
		/// </summary>
		/// <param name="component">The interaction that occurred</param>
		public void HandleComponent(SocketMessageComponent component)
		{
			_ = Task.Run(async () =>
			{
				try
				{
					await HandleComponentAsync(component);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Error occurred while resolving component message: {component.Message.Id}::{component.User.Id}");
				}
			});
		}

		/// <summary>
		/// Handles the given socket component interaction
		/// </summary>
		/// <param name="component">The interaction that occurred</param>
		/// <returns>A task representing the completion of the interaction</returns>
		public async Task HandleComponentAsync(SocketMessageComponent component)
		{
			if (component.User.IsBot ||
				component.User.Id == _client.CurrentUser.Id)
			{
				await component.DeferAsync(ephemeral: true);
				return;
			}

			var id = component.Data.CustomId;
			if (!_handlers.GetHandler(id, out var method, out var type))
			{
				_logger.LogWarning($"Cannot find loaded button for `{id}`");
				await component.DeferAsync(ephemeral: true);
				return;
			}

			if (method == null || type == null)
			{
				_logger.LogWarning($"Method or type is null for `{id}`");
				await component.DeferAsync(ephemeral: true);
				return;
			}

			var service = _provider.GetService(type);
			if (service == null)
			{
				_logger.LogWarning($"Cannot find service for type `{type.Name}`");
				await component.DeferAsync(ephemeral: true);
				return;
			}

			((ComponentHandler)service).SetContext(component, _client);

			var res = method.Invoke(service, Array.Empty<object>());
			if (res == null) return;

			if (method.ReturnType == typeof(Task) ||
				method.ReturnType == typeof(Task<>))
				await (Task)res;
		}
	}
}
