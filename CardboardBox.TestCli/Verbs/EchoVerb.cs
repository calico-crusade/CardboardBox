using CommandLine;

namespace CardboardBox.TestCli.Verbs
{
	[Verb("echo", HelpText = "This is a test command")]
	public class EchoVerbOptions
	{
		[Option('u', "uppercase", Default = false, HelpText = "Whether or not to upper case everything")]
		public bool Uppercase { get; set; } = false;

		[Value(0, Required = true, HelpText = "The message that is echoed back")]
		public string Message { get; set; } = string.Empty;
	}

	public class EchoVerb : IVerb<EchoVerbOptions>
	{
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
