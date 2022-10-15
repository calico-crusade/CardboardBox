using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox.Setup.CliParser
{
	public interface ICommandLineBuilder
	{
		/// <summary>
		/// A collection of all of the verbs registered
		/// </summary>
		IReadOnlyCollection<CommandLineBuilder.CommandVerb> Verbs { get; }

		/// <summary>
		/// The exit code to return when the process has executed successfully
		/// </summary>
		int ExitCodeSuccess { get; }

		/// <summary>
		/// The exit code to return when the process has failed to execute
		/// </summary>
		int ExitCodeFailure { get; }

		/// <summary>
		/// Adds the given verb handler to the system
		/// </summary>
		/// <typeparam name="T">The class representing the verb handler</typeparam>
		/// <typeparam name="TOpt">The class representing the options for the verb</typeparam>
		/// <returns>The current instance of the builder for fluent chaining</returns>
		ICommandLineBuilder Add<T, TOpt>() where T : class, IVerb<TOpt> where TOpt : class;

		/// <summary>
		/// Sets the expected exit codes for successes and failures
		/// </summary>
		/// <param name="success">The exit code for a successful execution (defaults to 0)</param>
		/// <param name="failure">The exit code for a failure (defaults to 1)</param>
		/// <returns>The current instance of the builder for fluent chaining</returns>
		ICommandLineBuilder ExitCode(int success = 0, int failure = 1);
	}

	public class CommandLineBuilder : ICommandLineBuilder
	{
		private readonly IServiceCollection _services;
		private readonly List<CommandVerb> _verbs = new();

		/// <summary>
		/// The exit code to return when the process has executed successfully
		/// </summary>
		public int ExitCodeSuccess { get; set; } = 0;

		/// <summary>
		/// The exit code to return when the process has failed to execute
		/// </summary>
		public int ExitCodeFailure { get; set; } = 1;

		/// <summary>
		/// A collection of all of the verbs registered
		/// </summary>
		public IReadOnlyCollection<CommandVerb> Verbs => _verbs.AsReadOnly();

		public CommandLineBuilder(IServiceCollection services) 
		{
			_services = services ?? throw new ArgumentNullException(nameof(services));
		}

		/// <summary>
		/// Adds the given verb handler to the system
		/// </summary>
		/// <typeparam name="T">The class representing the verb handler</typeparam>
		/// <typeparam name="TOpt">The class representing the options for the verb</typeparam>
		/// <returns>The current instance of the builder for fluent chaining</returns>
		public ICommandLineBuilder Add<T, TOpt>() where T: class, IVerb<TOpt> where TOpt: class
		{
			_verbs.Add(new CommandVerb(typeof(TOpt), typeof(IVerb<TOpt>)));
			_services.AddTransient<IVerb<TOpt>, T>();
			return this;
		}

		/// <summary>
		/// Sets the expected exit codes for successes and failures
		/// </summary>
		/// <param name="success">The exit code for a successful execution (defaults to 0)</param>
		/// <param name="failure">The exit code for a failure (defaults to 1)</param>
		/// <returns>The current instance of the builder for fluent chaining</returns>
		public ICommandLineBuilder ExitCode(int success = 0, int failure = 1)
		{
			ExitCodeSuccess = success;
			ExitCodeFailure = failure;
			return this;
		}

		public record class CommandVerb(Type Options, Type VerbService);
	}
}
