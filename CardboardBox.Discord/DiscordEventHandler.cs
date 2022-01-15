using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardboardBox.Discord
{
	public interface IDiscordEventHandler
	{
		/// <summary>
		/// Adds a handler that requires an <see cref="IServiceProvider"/>
		/// </summary>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		IDiscordEventHandler AddHandler(Func<IServiceProvider, Task> handler);

		/// <summary>
		/// Adds a handler that requires an <see cref="IServiceProvider"/> and a <see cref="DiscordSocketClient"/>
		/// </summary>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		IDiscordEventHandler AddHandler(Func<IServiceProvider, DiscordSocketClient, Task> handler);

		/// <summary>
		/// Adds a handler that requires the given service type
		/// </summary>
		/// <typeparam name="T">The type of service to use</typeparam>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		IDiscordEventHandler AddHandler<T>(Func<T, Task> handler);

		/// <summary>
		/// Adds a handler that requires the given service type and a <see cref="DiscordSocketClient"/>
		/// </summary>
		/// <typeparam name="T">The type of service to use</typeparam>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		/// <exception cref="NullReferenceException">Thrown if the required service cannot be found within the DI collections</exception>
		IDiscordEventHandler AddHandler<T>(Func<T, DiscordSocketClient, Task> handler);

		/// <summary>
		/// Triggers all of the registered actions
		/// </summary>
		/// <param name="provider">The dependency injection service provider</param>
		/// <param name="client">The instance of the discord socket clinet</param>
		void Execute(IServiceProvider provider, DiscordSocketClient client);
	}

	public class DiscordEventHandler : IDiscordEventHandler
	{
		private readonly List<Func<IServiceProvider, DiscordSocketClient, Task>> _actions = new();

		/// <summary>
		/// Adds a handler that requires an <see cref="IServiceProvider"/>
		/// </summary>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		public IDiscordEventHandler AddHandler(Func<IServiceProvider, Task> handler)
		{
			return AddHandler((s, d) => handler(s));
		}

		/// <summary>
		/// Adds a handler that requires an <see cref="IServiceProvider"/> and a <see cref="DiscordSocketClient"/>
		/// </summary>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		public IDiscordEventHandler AddHandler(Func<IServiceProvider, DiscordSocketClient, Task> handler)
		{
			_actions.Add(handler);
			return this;
		}

		/// <summary>
		/// Adds a handler that requires the given service type
		/// </summary>
		/// <typeparam name="T">The type of service to use</typeparam>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		/// <exception cref="NullReferenceException">Thrown if the required service cannot be found within the DI collections</exception>
		public IDiscordEventHandler AddHandler<T>(Func<T, Task> handler)
		{
			return AddHandler<T>((s, d) => handler(s));
		}

		/// <summary>
		/// Adds a handler that requires the given service type and a <see cref="DiscordSocketClient"/>
		/// </summary>
		/// <typeparam name="T">The type of service to use</typeparam>
		/// <param name="handler">The handler to be triggered when the bot receives the "Client Ready" event.</param>
		/// <returns>The instance of <see cref="IDiscordEventHandler"/> for method chaining</returns>
		/// <exception cref="NullReferenceException">Thrown if the required service cannot be found within the DI collections</exception>
		public IDiscordEventHandler AddHandler<T>(Func<T, DiscordSocketClient, Task> handler)
		{
			return AddHandler(async (provider, client) =>
			{
				var service = provider.GetService<T>();
				if (service == null)
					throw new NullReferenceException($"Cannot find instance of `{typeof(T).Name}` in dependency collection. Did you register it?");

				await handler(service, client);
			});
		}

		/// <summary>
		/// Triggers all of the registered actions
		/// </summary>
		/// <param name="provider">The dependency injection service provider</param>
		/// <param name="client">The instance of the discord socket clinet</param>
		public void Execute(IServiceProvider provider, DiscordSocketClient client)
		{
			_ = Task.Run(async () =>
			{
				foreach (var action in _actions)
					await action(provider, client);
			});
		}
	}
}
