using Discord;
using Discord.WebSocket;

namespace CardboardBox.Discord.Components
{
	/// <summary>
	/// Represents a handler for <see cref="MessageComponent"/>
	/// </summary>
	public abstract class ComponentHandler
	{
		private SocketMessageComponent? _component;
		private IUser? _user;
		private IChannel? _channel;
		private IMessage? _message;
		private DiscordSocketClient? _client;

		/// <summary>
		/// Represents the <see cref="SocketMessageComponent"/> that triggered this interaction
		/// </summary>
		public SocketMessageComponent? Component => _component;

		/// <summary>
		/// Quick accessor to the data for the component interaction
		/// </summary>
		public SocketMessageComponentData? Data => Component?.Data;

		/// <summary>
		/// Quick accessor for the values for the component interaction
		/// </summary>
		public IReadOnlyCollection<string> Values => Data?.Values ?? Array.Empty<string>();

		/// <summary>
		/// The first value from <see cref="Values"/>
		/// </summary>
		public string? Value => Values.FirstOrDefault();
		
		/// <summary>
		/// The type of component that is being interacted with
		/// </summary>
		public ComponentType? Type => Data?.Type;

		/// <summary>
		/// The message that the component is attached to
		/// </summary>
		public IMessage? Message => _message ?? Component?.Message;

		/// <summary>
		/// The Id of the <see cref="Message"/> the component is attached to
		/// </summary>
		public ulong? MessageId => Message?.Id;

		/// <summary>
		/// The user who triggered the interaction
		/// </summary>
		public IUser User => _user ?? Component?.User ?? throw new ArgumentNullException(nameof(_user));

		/// <summary>
		/// The Id of the user who triggered the interaction
		/// </summary>
		public ulong UserId => User.Id;

		/// <summary>
		/// The channel the interaction happened in
		/// </summary>
		public IChannel Channel => _channel ?? Component?.Channel ?? throw new ArgumentNullException(nameof(_channel));

		/// <summary>
		/// The Id of the guild the interaction happened in
		/// </summary>
		public ulong? GuildId => Channel != null && Channel is ITextChannel txt ? txt.GuildId : null;

		/// <summary>
		/// The <see cref="DiscordSocketClient"/> of the bot
		/// </summary>
		public DiscordSocketClient Client => _client ?? new();

		/// <summary>
		/// Acknowledges the interaction
		/// </summary>
		/// <param name="ephemeral">Whether or not to show the acknowledgement to all users (false) or just the user triggering the interaction(true)</param>
		/// <returns>A task representing the completion of the request</returns>
		public virtual Task Acknowledge(bool ephemeral = false) => Component?.DeferAsync(ephemeral) ?? Task.CompletedTask;

		/// <summary>
		/// Removes all of the components from the message
		/// </summary>
		/// <returns>A task representing the completion of the request</returns>
		public virtual Task RemoveComponents() => Component?.UpdateAsync(t => t.Components = null) ?? Task.CompletedTask;

		/// <summary>
		/// Removes all of the components and allows for editing the rest of the message
		/// </summary>
		/// <param name="edit">Any edits to do to the message</param>
		/// <returns>A task representing the completion of the request</returns>
		public virtual Task RemoveComponents(Action<MessageProperties> edit)
		{
			if (Component == null) return Task.CompletedTask;

			return Component.UpdateAsync(t =>
			{
				edit(t);
				t.Components = null;
			});
		}

		/// <summary>
		/// Updates the message
		/// </summary>
		/// <param name="edit">Any edits to do to the message</param>
		/// <returns>A task representing the completion of the request</returns>
		public virtual Task Update(Action<MessageProperties> edit) => Component?.UpdateAsync(edit) ?? Task.CompletedTask;
	
		/// <summary>
		/// Sets the context of the interaction (internal use)
		/// </summary>
		/// <param name="component">The component interaction</param>
		/// <param name="client">The discord client</param>
		public void SetContext(SocketMessageComponent component, DiscordSocketClient client)
		{
			_component = component;
			_client = client;
		}

		/// <summary>
		/// Sets the context of the interaction (interal use)
		/// </summary>
		/// <param name="channel">The channel the interaction was sent in</param>
		/// <param name="user">The user who triggered the interaction</param>
		/// <param name="client">The discord client</param>
		/// <param name="message">The message that triggered the interaction</param>
		public void SetContext(IChannel channel, IUser user, DiscordSocketClient client, IMessage? message = null)
		{
			_channel = channel;
			_user = user;
			_client = client;
			_message = message;
		}
	}
}
