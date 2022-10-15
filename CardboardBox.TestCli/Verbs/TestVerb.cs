using CommandLine;
using Microsoft.Extensions.Logging;

namespace CardboardBox.TestCli.Verbs
{
	[Verb("test", isDefault: true, HelpText = "This is a test verb")]
	public class TestVerbOptions
	{
		[Value(0, Required = true, HelpText = "The exit code to return")]
		public int Code { get; set; } = 0;
	}

	public class TestVerb : SyncVerb<TestVerbOptions>
	{
		private readonly ILogger _logger;

		public TestVerb(ILogger<TestVerb> logger)
		{
			_logger = logger;
		}

		public override int RunSync(TestVerbOptions options)
		{
			_logger.LogInformation("Dependency Injection Works! {0}", options.Code);
			Console.WriteLine("Hit!");
			return options.Code;
		}
	}
}
