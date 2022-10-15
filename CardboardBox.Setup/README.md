# CardboardBox.Setup
Helpful dependency injection and console application related extensions and features

## Installation
You can install the nuget package within Visual Studio. It targets .net 6.0 to take advantage of most of the new features within C# and .net

```
PM> Install-Package CardboardBox.Setup
```

## Setup
The package is split into 2 primary parts; Dependency Injection extensions and a command line verb parser / runner.

### Dependency Injection Extensions

#### Configuration Extensions:
Here are a collection of useful configuration extensions I use frequently.

```csharp
using CardboardBox;
using Microsoft.Extensions.DependencyInjection;

//Registers a new configuration object with the service collection
new ServiceCollection()
	.AddConfig(c => {
		//Adds the "appsettings.json" file to the configuration object
		//This method supports xml, json, and ini files.
		c.AddFile("appsettings.json", optional: false, reloadOnChange: true);

		//This is short hand for the above (this is mostly a personal preference of mine)
		c.AddAppSettings();
	});

//You can also get the builder IConfiguration object out of the register.
new ServiceCollection()
	.AddConfig(c => {
		...
	}, out IConfiguration config);


//Once you have the IConfiguration object, you can use the binding extensions to get specific config objects out:
var myOptions = config.Bind<MyOptionsClass>();

//You can also bind to pre-created objects too
var myOptions = new MyOptionsClass();
config.BindInstance(myOptions);

//Both of the above binding methods also allow for you to specify a section of the configuration file to bind against
var mySubOptions = config.Bind<MySubOptionsClass>("my:sub:options");
```

#### Logging Extensions:
I personally think Serilog is the best logging system for C#. I used to use nLog, but it was a bit cumbersome to setup, so I made the switch.

```csharp
using CardboardBox;
using Microsoft.Extensions.DependencyInjection;

//Register Serilog using my personal preference for logging methods:
//This registers the File and Console sinks. The file sink has a rolling interval of Hourly, and outputs to the "logs/logs.txt" path.
new ServiceCollection().AddSerilog();

//You can also configuration serilog yourself
new ServiceCollection()
	.AddSerilog(c => {
		c.WriteTo.File(Path.Combine("some-logs", "my-super-log.txt"), rollingInterval: RollingInterval.Minute);
	});
```

### Command Line Verb Parser / Runner
This is a helpful utility that lets you register verbs that can be trigger via specific command line arguments.
These are useful for creating helpful CLI utilities, as I often do when working on big projects and need things like migrations, or test functions for things.

It utilizes `CommandLineParser` under the hood and supports pretty much everything they do when it comes to verbs.

First you will need to create a verb handler:
```csharp
using CardboardBox;
using CommandLine;

namespace MySuperUtility 
{
	//This class represents the command line options from CommandLineParser
	[Verb("echo", isDefault: true, HelpText = "This echos the given message back to you")]
	public class EchoOptions 
	{
		[Option('u', "uppercase", Default = false, HelpText = "Whether or not to upper case everything")]
		public bool Uppercase { get; set; } = false;

		[Value(0, Required = true, HelpText = "The message that is echoed back")]
		public string Message { get; set; } = string.Empty;
	}

	//This class is what actually handles running the command
	public class EchoVerb : IVerb<EchoOptions> //This interface is important, otherwise the register won't be able to find your command
	{
		//This is the method that gets run when the verb is run
		public Task<int> Run(EchoVerbOptions options)
		{
			var msg = options.Message;
			if (options.Uppercase)
				msg = msg.ToUpper();

			Console.WriteLine(msg);
			return Task.FromResult(0);
		}
	}
}
```

Note: Out of the box, all `IVerb` instances support dependency injection, and their registration with the Service Collection is handled automatically!

Next you will need to register the verb with the register
```csharp
using CardboardBox;
using Microsoft.Extensions.DependencyInjection;
using MySuperUtility;

return await new ServiceCollection()
	.AddSerilog() //This ensures we get any warnings from the CLI system. Feel free to register any ILogger though.
	.Cli(c => 
	{
		//This tells the application about the verb and it's options, you can register as many of these as you want.
		c.Add<EchoVerb, EchoOptions>();
	});
```

