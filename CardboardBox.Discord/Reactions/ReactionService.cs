using Discord;
using Discord.WebSocket;

namespace CardboardBox.Discord
{
	using Microsoft.Extensions.Logging;
	using Reactions;

	public interface IReactionService
	{
		/// <summary>
		/// Executed when a reaction is added to a message
		/// </summary>
		/// <param name="message">The message the reaction was added to</param>
		/// <param name="channel">The channel the reaction was sent in</param>
		/// <param name="reaction">The reaction that occurred</param>
		/// <returns>A task representing the completion of the handler</returns>
		void ReactionAdded(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction);

		/// <summary>
		/// Adds a reactions handler to the given message
		/// </summary>
		/// <param name="message">The message to watch</param>
		/// <param name="reactions">The reactions handler</param>
		Task Reactions(IMessage message, Action<IReactionHandlerBuilder> reactions);

		/// <summary>
		/// Adds a reactions handler to the given message
		/// </summary>
		/// <param name="message">The message to watch</param>
		/// <returns>The reaction handlers</returns>
		IReactionHandlerBuilder Reactions(IMessage message);
	}

	public class ReactionService : IReactionService
	{
		private readonly Dictionary<ulong, IReactionHandler> _reactions;
		private readonly DiscordSocketClient _client;
		private readonly ILogger _logger;

		public ReactionService(
			DiscordSocketClient client,
			ILogger<ReactionService> logger)
		{
			_client = client;
			_reactions = new();
			_logger = logger;
		}

		/// <summary>
		/// Executed when a reaction is added to a message
		/// </summary>
		/// <param name="message">The message the reaction was added to</param>
		/// <param name="channel">The channel the reaction was sent in</param>
		/// <param name="reaction">The reaction that occurred</param>
		/// <returns>A task representing the completion of the handler</returns>
		public void ReactionAdded(
			Cacheable<IUserMessage, ulong> message,
			Cacheable<IMessageChannel, ulong> channel,
			SocketReaction reaction)
		{
			if (reaction.UserId == _client.CurrentUser.Id) return;
			if (_reactions.Count == 0 || !_reactions.ContainsKey(message.Id)) return;

			_ = Task.Run(async () =>
			{
				try
				{
					await _reactions[message.Id].Handle(reaction);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"An error occurred while attempting to handle a reaction: Message ID: {message.Id}");
				}
			});
		}

		/// <summary>
		/// Executed when reactions are finished being handled
		/// </summary>
		/// <param name="id">The ID of the message that was handled</param>
		public void ReactionFinished(ulong id)
		{
			if (_reactions.ContainsKey(id))
				_reactions.Remove(id);
		}

		/// <summary>
		/// Adds a reactions handler to the given message
		/// </summary>
		/// <param name="message">The message to watch</param>
		/// <param name="reactions">The reactions handler</param>
		public async Task Reactions(IMessage message, Action<IReactionHandlerBuilder> reactions)
		{
			var builder = (ReactionHandlerBuilder)Reactions(message);
			reactions(builder);
			await builder.AddReactions();
		}

		/// <summary>
		/// Adds a reactions handler to the given message
		/// </summary>
		/// <param name="message">The message to watch</param>
		/// <returns>The reaction handlers</returns>
		public IReactionHandlerBuilder Reactions(IMessage message)
		{
			var builder = new ReactionHandlerBuilder(message, () => ReactionFinished(message.Id));
			_reactions.Add(message.Id, builder);
			return builder;
		}
	}
}
