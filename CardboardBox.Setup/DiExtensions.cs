using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CardboardBox
{
	public static class DiExtensions
	{
		/// <summary>
		/// Adds Serilog to the service collection with the given configuration
		/// </summary>
		/// <param name="services">The service collection to add serilog to</param>
		/// <param name="bob">The configuration builder. (Get it? Bob the logger configuration builder! Hahaha. I'm Funny)</param>
		/// <returns>The service collection for fluent chaining</returns>
		public static IServiceCollection AddSerilog(this IServiceCollection services, Action<LoggerConfiguration> bob)
		{
			return services
				.AddLogging(c =>
				{
					var config = new LoggerConfiguration();
					bob?.Invoke(config);
					c.AddSerilog(config.CreateLogger());
				});
		}

		/// <summary>
		/// Adds Serilog with my personal preference of Sinks and configuration
		/// File Sink -> Puts logs in logs\log.text, with a new file every hour
		/// Console Sink -> Exports the logs to the console as well
		/// Minimum Level -> Debug to catch all of the logs
		/// </summary>
		/// <param name="services">The service collection to add serilog to</param>
		/// <returns>The service collection for fluent chaining</returns>
		public static IServiceCollection AddSerilog(this IServiceCollection services)
		{
			return services
				.AddSerilog(c => 
					c.WriteTo.Console()
					 .WriteTo.File(Path.Combine("logs", "log.txt"), rollingInterval: RollingInterval.Hour)
					 .MinimumLevel.Debug());
		}

		/// <summary>
		/// Adds a default "appsettings.json" config file (along with any extra configurations)
		/// </summary>
		/// <param name="services">The service collection to add the appsettings.json configurationt o</param>
		/// <param name="bob">The configuration builder. (Get it? Bob the configuration builder! Hahaha. I'm Funny)</param>
		/// <returns>The service collection for fluent chaining</returns>
		public static IServiceCollection AddAppSettings(this IServiceCollection services, Action<IConfigurationBuilder>? bob = null)
		{
			return services
				.AddConfig(c =>
				{
					c.AddFile("appsettings.json");
					bob?.Invoke(c);
				});
		}

		/// <summary>
		/// Adds the given file as a option to the Configuration builder
		/// Supported file types: JSON, XML, and INI
		/// </summary>
		/// <param name="bob">The configuration builder to bind the file to</param>
		/// <param name="filepath">The relative path to the configuration file</param>
		/// <param name="optional">Whether or not the file is optional</param>
		/// <param name="reloadOnChange">Whether or not to reload the configuration when a change is detected to the files contents</param>
		/// <returns>The configuration builder for fluent chaining</returns>
		/// <exception cref="ArgumentNullException">Thrown if the extension is not found, or if the extension is invalid</exception>
		public static IConfigurationBuilder AddFile(this IConfigurationBuilder bob, string filepath, bool optional = false, bool reloadOnChange = true)
		{
			var ext = Path.GetExtension(filepath).TrimStart('.').ToLower();
			if (string.IsNullOrWhiteSpace(ext)) throw new ArgumentNullException(nameof(filepath), "No valid extension found!");

			Func<string, bool, bool, IConfigurationBuilder> builder = ext switch
			{
				"json" => bob.AddJsonFile,
				"xml" => bob.AddXmlFile,
				"ini" => bob.AddIniFile,
				_ => throw new ArgumentNullException(nameof(filepath), $"Couldn't find a valid loader for extension \"{ext}\"!")
			};

			return builder(filepath, !optional, reloadOnChange);
		}

		/// <summary>
		/// Binds the given configuration object to an instance of the given type
		/// </summary>
		/// <typeparam name="T">The type of object to bind to</typeparam>
		/// <param name="config">The configuration object to bind from</param>
		/// <param name="section">The optional configuration section to bind from</param>
		/// <returns>The bound instance of the object</returns>
		public static T Bind<T>(this IConfiguration config, string? section = null) => config.BindInstance(Activator.CreateInstance<T>(), section);

		/// <summary>
		/// Binds the given configuration object to the given instance
		/// </summary>
		/// <typeparam name="T">The type of object to bind to</typeparam>
		/// <param name="config">The configuration option to bind from</param>
		/// <param name="instance">The object to bind the configuration object to</param>
		/// <param name="section">The optional configuration section to bind from</param>
		/// <returns>The bound instance of the object</returns>
		public static T BindInstance<T>(this IConfiguration config, T instance, string? section = null)
		{
			var sec = config;
			if (!string.IsNullOrEmpty(section))
				sec = config.GetSection(section);

			sec.Bind(instance);
			return instance;
		}

		/// <summary>
		/// Adds an <see cref="IConfiguration"/> instance to the given service provider
		/// </summary>
		/// <param name="services">The service collection to add the configuration object to</param>
		/// <param name="bob">The configuration builder. (Get it? Bob the configuration builder! Hahaha. I'm Funny)</param>
		/// <param name="config">The created <see cref="IConfiguration"/> object</param>
		/// <returns>The service collection for fluent chaining</returns>
		public static IServiceCollection AddConfig(this IServiceCollection services, Action<IConfigurationBuilder> bob, out IConfiguration config)
		{
			var builder = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.AddCommandLine(Environment.GetCommandLineArgs());

			bob?.Invoke(builder);

			config = builder.Build();
			return services.AddSingleton(config);
		}

		/// <summary>
		/// Adds an <see cref="IConfiguration"/> instance to the given service provider
		/// </summary>
		/// <param name="services">The service collection to add the configuration object to</param>
		/// <param name="bob">The configuration builder. (Get it? Bob the configuration builder! Hahaha. I'm Funny)</param>
		/// <returns>The service collection for fluent chaining</returns>
		public static IServiceCollection AddConfig(this IServiceCollection services, Action<IConfigurationBuilder> bob) => services.AddConfig(bob, out _);
	}
}