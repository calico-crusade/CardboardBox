# CardboardBox.Discord
Extension onto Discord.Net to add the ability to use Discord Slash/Application Commands

## Installation
You can install the nuget package within Visual Studio. It targets .net 6.0 to take advantage of most of the new features within C# and .net.

```
PM> Install-Package CardboardBox.Discord
```

## Setup
You can setup the Discord bot using the following code:

In your Program.cs file:
```csharp
using CardboardBox.Discord;
using Microsoft.Extensions.DependencyInjection;

await DiscordBotBuilder.Start()
	.WithServices(c => 
	{
		//Register any dependency injection services
	})
	.WithSlashCommands(c =>
	{
		//Register any slash commands
	})
	.Build()
	.Login();

//This is important to stop the console application from closing
await Task.Delay(-1);
```

## Slash Commands
You can setup a slash command in one of two ways. You can either use the fluent API or use attributes.

My preference is the attributes, however, specifying choices on your options can be rather tedious using attributes. However, I am working on an alternative form that will allow for the best of both worlds.

All of the classes used for commands have dependency injection enabled by default, however, they do not need to be registered with the `IServiceCollection`, as the command registeration process takes care of that for you.

### Attribute Slash Commands
Attribute based slash commands can be used by creating a class and decorating the methods with attributes.

```csharp
using CardboardBox.Discord;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ExampleClient 
{
	public class BotCommands 
	{
		private readonly ILogger _logger;

		public BotCommands(ILogger<BotCommands> logger)
		{
			_logger = logger;
		}

		[Command("ping", "Checks to see if the bot is alive")]
		public async Task Ping(SocketSlashCommand cmd)
		{
			_logger.LogInformation("The bot is alive!");
			await cmd.RespondAsync("Pong!", ephemeral: true);
		}

		[Command("echo", "Echos the given message")]
		public async Task Echo(SocketSlashCommand cmd, [Option("The message to echo", true)] string message)
		{
			await cmd.Modify($"You said `{message}`.");
		}
	}
}
```

Then in your Program.cs, register the `BotCommand` class into your services

```csharp
...
.WithSlashCommands(c => 
{
	//Register the commands with the bot
	c.With<BotCommands>();
})
...
```

### Fluent Slash Commands
Fluent Slash Commands can be used without attributes and allows more configuration.

```csharp
using CardboardBox.Discord;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace ExampleClient 
{
	public class BotCommands 
	{
		private readonly ILogger _logger;

		public BotCommands(ILogger<BotCommands> logger)
		{
			_logger = logger;
		}

		public async Task Ping(SocketSlashCommand cmd)
		{
			_logger.LogInformation("The bot is alive!");
			await cmd.RespondAsync("Pong!", ephemeral: true);
		}

		public async Task Echo(SocketSlashCommand cmd)
		{
			var message = (string?)cmd.Data.Options.FirstOrDefault()?.Value;
			await cmd.Modify($"You said `{message}`.");
		}
	}
}
```

Then in your Program.cs, register the `BotCommands.Echo` and `BotCommands.Ping` methods into your services

```csharp
...
.WithSlashCommands(c => 
{
	//Register the commands with the bot
	c.With<BotCommands>("ping", t => t.Ping, b => 
	{
		b.WithDescription("Checks to see if the bot is alive");
	});

	c.With<BotCommands>("echo", t => t.Echo, b => 
	{
		b.WithDescription("Echos the given message")
		 .AddOption("message", Discord.ApplicationCommandOptionType.String, "The message to echo", true);
	});
})
...
```

## Notes
Other than that, the bot works like any other Discord.Net instance. You can use regular commands with it as well.

This library does not support sub-commands for now. Only top level commands.