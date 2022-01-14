using CardboardBox.Discord;
using CardboardBox.Discord.TestCli;

await DiscordBotBuilder.Start()
	.WithSlashCommands(c =>
	{
		c.With<TestCommands>()
		 .WithComponent<SkyrimButtons>();
	})
	.Build()
	.Login();

await Task.Delay(-1);