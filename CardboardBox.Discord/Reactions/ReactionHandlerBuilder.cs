using Discord;
using Discord.WebSocket;

namespace CardboardBox.Discord.Reactions
{
	public interface IReactionHandlerBuilder
	{
		/// <summary>
		/// Whether or not allow the reaction handler to be executed more than once
		/// </summary>
		bool ExecuteOnce { get; }

		/// <summary>
		/// Whether or not to clear the reactions from the message once it's been handled.
		/// </summary>
		bool ClearReactionsOnFinish { get; }

		/// <summary>
		/// Registers a handler for a specific emote
		/// </summary>
		/// <param name="emote">The emote to handle</param>
		/// <param name="react">How the bot should respond to this emote</param>
		/// <param name="filter">Allows declaring a filter so the reaction handler will only execute if the filter succeeds</param>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		IReactionHandlerBuilder React(string emote, Func<SocketReaction, Task> react, Func<SocketReaction, bool>? filter = null);

		/// <summary>
		/// Registers a handler for a specific emote
		/// </summary>
		/// <param name="emote">The emote to handle</param>
		/// <param name="react">How the bot should respond to this emote</param>
		/// <param name="filter">Allows declaring a filter so the reaction handler will only execute if the filter succeeds</param>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		IReactionHandlerBuilder React(IEmote emote, Func<SocketReaction, Task> react, Func<SocketReaction, bool>? filter = null);

		/// <summary>
		/// Whether or not to allow the reaction handler to be executed multiple times or only once
		/// </summary>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		IReactionHandlerBuilder AllowMultipleExecutions();

		/// <summary>
		/// Whether or not to preserve the reactions or delete them once they've been handled.
		/// </summary>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		IReactionHandlerBuilder PreserveReactions();

		/// <summary>
		/// Allows declaring a filter so the reaction handler is only executed if the filter succeeds.
		/// </summary>
		/// <param name="filter">The filter to execute</param>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		IReactionHandlerBuilder OnlyOn(Func<SocketReaction, bool> filter);

		/// <summary>
		/// Returns a task that will complete once the reaction handler is completed.
		/// </summary>
		/// <returns>The task that represents the completion of the reaction handler</returns>
		/// <exception cref="InvalidOperationException"></exception>
		Task<SocketReaction> AsTask();

		/// <summary>
		/// Returns a task that will complete once the reaction handler is completed.
		/// </summary>
		/// <param name="token">The <see cref="CancellationToken"/> for the task</param>
		/// <returns>The task that represents the completion of the reaction handler</returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="ExecuteOnce"/> is set to false</exception>
		Task<SocketReaction> AsTask(out CancellationToken token);

		/// <summary>
		/// Adds all of the reactions to the message
		/// </summary>
		/// <returns>A task representing the completion of adding the reactions</returns>
		Task AddReactions();
	}

	public interface IReactionHandler : IReactionHandlerBuilder
	{
		/// <summary>
		/// Executes the reaction handler for the given reaction
		/// </summary>
		/// <param name="reaction">The reaction</param>
		/// <returns>A task representing whether or not the reaction was successfully handled</returns>
		Task<bool> Handle(SocketReaction reaction);
	}

	public class ReactionHandlerBuilder : IReactionHandler
	{
		private readonly IMessage _message;
		private readonly Action _onFinished;
		private readonly Dictionary<IEmote, (Func<SocketReaction, Task>, Func<SocketReaction, bool>?)> _reactions = new();
		private TaskCompletionSource<SocketReaction>? _taskSource = null;
		private Func<SocketReaction, bool>? _filter = null;

		/// <summary>
		/// Whether or not allow the reaction handler to be executed more than once
		/// </summary>
		public bool ExecuteOnce { get; set; } = true;

		/// <summary>
		/// Whether or not to clear the reactions from the message once it's been handled.
		/// </summary>
		public bool ClearReactionsOnFinish { get; set; } = true;

		public ReactionHandlerBuilder(
			IMessage message, 
			Action onFinished)
		{
			_message = message;
			_onFinished = onFinished;
		}

		/// <summary>
		/// Checks to see if the given emote has a reaction handler
		/// </summary>
		/// <param name="emote">The emote to check against</param>
		/// <param name="reaction">The reaction handler</param>
		/// <returns>Whether or not the reaction exists</returns>
		public bool ReactionExists(IEmote emote, out (Func<SocketReaction, Task>?, Func<SocketReaction, bool>?) reaction)
		{
			reaction = (null, null);
			var current = _reactions
				.Where(t => t.Key.Name == emote.Name)
				.ToArray();
			if (current.Length == 0) return false;

			var (_, r) = current.First();
			reaction = r;
			return true;
		}

		/// <summary>
		/// Whether or not to allow the reaction handler to be executed multiple times or only once
		/// </summary>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		public IReactionHandlerBuilder AllowMultipleExecutions()
		{
			ExecuteOnce = false;
			return this;
		}

		/// <summary>
		/// Whether or not to preserve the reactions or delete them once they've been handled.
		/// </summary>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		public IReactionHandlerBuilder PreserveReactions()
		{
			ClearReactionsOnFinish = false;
			return this;
		}

		/// <summary>
		/// Allows declaring a filter so the reaction handler is only executed if the filter succeeds.
		/// </summary>
		/// <param name="filter">The filter to execute</param>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		public IReactionHandlerBuilder OnlyOn(Func<SocketReaction, bool> filter)
		{
			_filter = filter;
			return this;
		}

		/// <summary>
		/// Registers a handler for a specific emote
		/// </summary>
		/// <param name="emote">The emote to handle</param>
		/// <param name="react">How the bot should respond to this emote</param>
		/// <param name="filter">Allows declaring a filter so the reaction handler will only execute if the filter succeeds</param>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		public IReactionHandlerBuilder React(string emote, Func<SocketReaction, Task> react, Func<SocketReaction, bool>? filter = null)
		{
			var e = GetEmote(emote);
			return React(e, react, filter);
		}

		/// <summary>
		/// Registers a handler for a specific emote
		/// </summary>
		/// <param name="emote">The emote to handle</param>
		/// <param name="react">How the bot should respond to this emote</param>
		/// <param name="filter">Allows declaring a filter so the reaction handler will only execute if the filter succeeds</param>
		/// <returns>The current instance of <see cref="IReactionHandlerBuilder"/> for method chaining</returns>
		public IReactionHandlerBuilder React(IEmote emote, Func<SocketReaction, Task> react, Func<SocketReaction, bool>? filter = null)
		{
			if (ReactionExists(emote, out _))
				throw new InvalidOperationException($"There is already a registered reaction handler for: " + emote.Name);

			_reactions.Add(emote, (react, filter));
			return this;
		}

		/// <summary>
		/// Adds all of the reactions to the message
		/// </summary>
		/// <returns>A task representing the completion of adding the reactions</returns>
		public async Task AddReactions()
		{
			foreach(var emote in _reactions.Keys)
				await _message.AddReactionAsync(emote);
		}

		/// <summary>
		/// Returns a task that will complete once the reaction handler is completed.
		/// </summary>
		/// <returns>The task that represents the completion of the reaction handler</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public Task<SocketReaction> AsTask()
		{
			return AsTask(out _);
		}

		/// <summary>
		/// Returns a task that will complete once the reaction handler is completed.
		/// </summary>
		/// <param name="token">The <see cref="CancellationToken"/> for the task</param>
		/// <returns>The task that represents the completion of the reaction handler</returns>
		/// <exception cref="InvalidOperationException">Thrown if <see cref="ExecuteOnce"/> is set to false</exception>
		public Task<SocketReaction> AsTask(out CancellationToken token)
		{
			if (!ExecuteOnce)
				throw new InvalidOperationException("Cannot execute task on a reaction handler that can execute more than once");

			token = new CancellationToken();
			_taskSource = new TaskCompletionSource<SocketReaction>(token);
			return _taskSource.Task;
		}

		/// <summary>
		/// Executes the reaction handler for the given reaction
		/// </summary>
		/// <param name="reaction">The reaction</param>
		/// <returns>A task representing whether or not the reaction was successfully handled</returns>
		public async Task<bool> Handle(SocketReaction reaction)
		{
			//Make sure we're on the correct message
			if (_message.Id != reaction.MessageId) return false;

			//Make sure we match the global filter
			if (_filter != null && !_filter(reaction)) return false;
			
			//Make sure the reaction is within the handled reactions
			if (!ReactionExists(reaction.Emote, out var reactParts)) return false;

			var (react, filter) = reactParts;

			//Make sure the reaction isn't null
			if (react == null) return false;

			//Ensure we match the specific filter
			if (filter != null && !filter(reaction)) return false;

			//Execute the reaction
			await react(reaction);

			//If the reaction is set to be used multiple times, we can skip the rest of the steps
			if (!ExecuteOnce) return true;

			//Clear the reactions if necessary
			if (ClearReactionsOnFinish)
				await _message.RemoveAllReactionsAsync();

			//Finish the task's execution if necessary
			if (_taskSource != null)
				_taskSource.SetResult(reaction);

			//Make the reaction as finished
			_onFinished();

			return true;
		}

		/// <summary>
		/// Returns the correct <see cref="IEmote"/> for the given emote
		/// </summary>
		/// <param name="emote">The string representation of the emote</param>
		/// <returns>The correct <see cref="IEmote"/></returns>
		/// <exception cref="InvalidDataException">Thrown if the emote is not of type <see cref="Emote"/> or <see cref="Emoji"/></exception>
		public IEmote GetEmote(string emote)
		{
			if (Emote.TryParse(emote, out var e))
				return e;

			if (Emoji.TryParse(emote, out var ej))
				return ej;

			throw new InvalidDataException($"'{emote}' is not a valid emote or emoji");
		}
	}
}
