using CardboardBox.Discord.Components;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace CardboardBox.Discord.TestCli
{
	public class TestCommands
	{
		public const ulong DISCORD_TEST_GUILD_ID = 1009959054073933885; //318239174530695169; //829412577426538537;

		private readonly IReactionService _reactions;
		private readonly IComponentService _component;
		private readonly IModalService _modal;
		private readonly DiscordSocketClient _client;
		private readonly ILogger _logger;

		public TestCommands(
			IReactionService reactions,
			IComponentService component,
			IModalService modal,
			DiscordSocketClient client,
			ILogger<TestCommands> logger)
		{
			_reactions = reactions;
			_client = client;
			_component = component;
			_logger = logger;
			_modal = modal;
		}

		[GuildCommand("test-reactions", "Reactions capabilities", DISCORD_TEST_GUILD_ID)]
		public async Task TestReactions(SocketSlashCommand cmd)
		{
			var msg = await cmd.Respond("This is a test!");

			await _reactions.Reactions(msg, t =>
			{
				t
				//.OnlyOn(a => a.UserId == cmd.User.Id)
				.React("✔️", async reaction =>
				{
					await msg.ModifyAsync(t => t.Content = "This has been a test, good night.");
				}, a => a.UserId == cmd.User.Id)
				.React("🎅", async reaction =>
				{
					await msg.ModifyAsync(t => t.Content = "Ho ho ho!");
				})
				.React("<:5head:614356868134993921>", async reaction =>
				{
					await msg.ModifyAsync(t => t.Content = "That's a large forehead you have there.");
				})
				.React("🤶", async reaction =>
				{
					await msg.ModifyAsync(t => t.Content = "Mrs. Clause, can I have a cookie?");
				})
				.AllowMultipleExecutions();
			});
		}

		[GuildCommand("test-buttons", "Button Capabilities", DISCORD_TEST_GUILD_ID)]
		public async Task TestButtons(SocketSlashCommand cmd,
			[Option("number", true)] double number)
		{
			var builder = await _component.Components<SkyrimButtons>(cmd);

			await cmd.Respond("This is a button: " + number, components: builder);


			var msg = await cmd.Respond("Hello world");
			var components = await _component.Components<SkyrimButtons>(msg);
			await msg.ModifyAsync(t => t.Components = components);
			//_state.Set(msg, <state>);
		}

		[GuildCommand("test-ephemeral", "Testing Ephemeral Capabilities", DISCORD_TEST_GUILD_ID, Ephemeral = true, LongRunning = true)]
		public async Task EphermalTest(SocketSlashCommand cmd)
		{
			_logger.LogInformation("Waiting...");
			await Task.Delay(5000);
			_logger.LogInformation("Waited!");
			await cmd.Modify("Hello world, how are you?");
		}

		[GuildCommand("test-modal", "Testing Modals", DISCORD_TEST_GUILD_ID, Ephemeral = true)]
		public async Task TestModal(SocketSlashCommand cmd)
        {
            var modal = _modal.Modal<TestCommands>(t => t.SomeTextModal);
			await cmd.RespondWithModalAsync(modal);
		}

		[Modal("What is your favourite:")]
		public async Task SomeTextModal(SocketModal modal,
			[Text("Band:", "It better not be J-Pop...", Row = 1)] string band,
			[Text("Quote:", "Is it some weeb shit?", TextInputStyle.Paragraph, Value = "The path to hell is paved with good intentions.", Row = 2)] string? quote)
		{
			await modal.RespondAsync($"So, here's what I learnt:\r\nBand: {band}\r\nQuote: {quote}");
		}
    }
	
	public class SkyrimButtons : ComponentHandler
	{
		[Button("Law", "👮‍♂️", ButtonStyle.Danger, Row = 3)]
		public async Task LawButton()
		{
			await RemoveComponents(t => t.Content = "Stop! You have violated the law! PAY THE COURT A FINE OR SERVE YOUR SENTENCE!");
		}

		[Button("NO!", "<a:MODS:867203260371697664>", Row = 3)]
		public async Task No()
		{
			await Update(t => t.Content = "https://tenor.com/view/michael-scott-the-office-uh-oh-no-gif-12741203");
		}

		[SelectMenu(Row = 1)]
		[DefaultSelectMenuOption("I'm fine, how are you?", "fine")]
		[SelectMenuOption("Pretty shit", "bad")]
		[SelectMenuOption("horrible")]
		public async Task HowAreYou()
		{
			await RemoveComponents(t => t.Content = $"Well. I'm doing fine. Hopefully `{Value}` isn't a bad thing.");
		}

		[SelectMenu(nameof(SomethingElseOptions), Row = 2, MaxValues = 2, MinValues = 1)]
		public async Task SomethingElse()
		{
			await RemoveComponents(t => t.Content = $"I see your {string.Join(", ", Values)}");
		}

		[SelectMenu(ComponentType.UserSelect, Row = 4, Placeholder = "Select a user")]
		public async Task MentionableInput()
		{
			await RemoveComponents(t => t.Content = $"I see your `{string.Join(", ", Values)}` - mention input");
		}

		[SelectMenu(ComponentType.ChannelSelect, ChannelType.Forum, ChannelType.Voice, Row = 5, Placeholder = "Select a channel")]
		public async Task UserInput()
		{
			await RemoveComponents(t => t.Content = $"I see your `{string.Join(", ", Values)}` - user input");
		}

		public Task<SelectMenuOptionBuilder[]> SomethingElseOptions()
		{
			var options = new List<SelectMenuOptionBuilder>();

			for (var i = 0; i < 10; i++)
				options.Add(new SelectMenuOptionBuilder()
					.WithLabel($"Test-{i}-{User?.Username ?? ("NO USER FOUND")}")
					.WithValue(i.ToString()));

			return Task.FromResult(options.ToArray());
		}
	}
}
