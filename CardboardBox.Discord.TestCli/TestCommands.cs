using CardboardBox.Discord.Components;
using Discord;
using Discord.WebSocket;

namespace CardboardBox.Discord.TestCli
{
	public class TestCommands
	{
		public const ulong DISCORD_TEST_GUILD_ID = 318239174530695169; //829412577426538537;

		private readonly IReactionService _reactions;
		private readonly IComponentService _component;
		private readonly DiscordSocketClient _client;

		public TestCommands(
			IReactionService reactions,
			IComponentService component,
			DiscordSocketClient client)
		{
			_reactions = reactions;
			_client = client;
			_component = component;
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
		}
	}
	
	public class SkyrimButtons : ComponentHandler
	{
		[Button("Law", "👮‍♂️", ButtonStyle.Danger)]
		public async Task LawButton()
		{
			await RemoveComponents(t => t.Content = "Stop! You have violated the law! PAY THE COURT A FINE OR SERVE YOUR SENTENCE!");
		}

		[Button("NO!", "<a:MODS:867203260371697664>")]
		public async Task No()
		{
			await Update(t => t.Content = "https://tenor.com/view/michael-scott-the-office-uh-oh-no-gif-12741203");
		}

		[SelectMenu]
		[DefaultSelectMenuOption("I'm fine, how are you?", "fine")]
		[SelectMenuOption("Pretty shit", "bad")]
		[SelectMenuOption("horrible")]
		public async Task HowAreYou()
		{
			await RemoveComponents(t => t.Content = $"Well. I'm doing fine. Hopefully `{Value}` isn't a bad thing.");
		}

		[SelectMenu(nameof(SomethingElseOptions), Row = 1, MaxValues = 2, MinValues = 1)]
		public async Task SomethingElse()
		{
			await RemoveComponents(t => t.Content = $"I see your {string.Join(", ", Values)}");
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
