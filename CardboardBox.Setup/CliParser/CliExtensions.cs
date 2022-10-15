using Microsoft.Extensions.DependencyInjection;

namespace CardboardBox
{
	using Setup.CliParser;

	public static class CliExtensions
	{
		/// <summary>
		/// Triggers the handling of command line verbs against the given service collection and command line arguments
		/// </summary>
		/// <param name="services">The service collection to use for dependency injection</param>
		/// <param name="args">The command line arguments to use</param>
		/// <param name="bob">The command line parser configuration builder</param>
		/// <returns>The exit code returned by the executed verb</returns>
		public static Task<int> Cli(this IServiceCollection services, string[] args, Action<ICommandLineBuilder> bob)
		{
			var builder = new CommandLineBuilder(services);
			bob?.Invoke(builder);

			var provider = services
				.AddSingleton<ICommandLineBuilder>(builder)
				.AddTransient<ICommandLineService, CommandLineService>()
				.BuildServiceProvider();

			services.AddSingleton(provider);

			var srv = provider.GetRequiredService<ICommandLineService>();
			return srv.Run(args);
		}

		/// <summary>
		/// Triggers the handling of command line verbs against the given service collection and command line arguments
		/// </summary>
		/// <param name="services">The service collection to use for dependency injection</param>
		/// <param name="bob">The command line parser configuration builder</param>
		/// <param name="skipFirst">Whether or not to skip the first argument as it is usually the current executables path</param>
		/// <returns>The exit code returned by the executed verb</returns>
		public static Task<int> Cli(this IServiceCollection services, Action<ICommandLineBuilder> bob, bool skipFirst = true)
		{
			var args = Environment.GetCommandLineArgs();

			if (skipFirst)
				args = args.Skip(1).ToArray();
			return services.Cli(args, bob);
		}
	}
}
