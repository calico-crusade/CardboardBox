using CardboardBox.Discord;
using CardboardBox.Discord.TestCli;

await DiscordBotBuilder.Start()
	.WithSlashCommands(c =>
	{
		c.With<TestCommands>();
	})
	.Build()
	.Login();

await Task.Delay(-1);