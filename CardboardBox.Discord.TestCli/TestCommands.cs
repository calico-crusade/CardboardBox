using Discord.WebSocket;

namespace CardboardBox.Discord.TestCli
{
	public class TestCommands
	{
		public const ulong DISCORD_TEST_GUILD_ID = 829412577426538537;

		private readonly IReactionService _reactions;
		private readonly DiscordSocketClient _client;

		public TestCommands(
			IReactionService reactions,
			DiscordSocketClient client)
		{
			_reactions = reactions;
			_client = client;
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
	}
}
