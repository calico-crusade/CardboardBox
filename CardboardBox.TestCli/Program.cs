using Microsoft.Extensions.DependencyInjection;
using CardboardBox;
using CardboardBox.TestCli.Verbs;

return await new ServiceCollection()
	.AddSerilog()
	.Cli(c =>
	{
		c.Add<TestVerb, TestVerbOptions>()
		 .Add<EchoVerb, EchoVerbOptions>();
	});

