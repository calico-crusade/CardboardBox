# CardboardBox.Setup
Used to wrap different types of dependency injection systems. Allows CardboardBox projects to work with [StructureMap](http://structuremap.github.io/), [Lamar](https://jasperfx.github.io/lamar/), or .net core's ```IServiceCollection``` system.

This was mostly made because a lot of my older .net framework projects that I've ported to .net core used StructureMap, this allows me to still use CardboardBox with those projects while also supporting the latest and greatest IoC libraries.

## Features
* Settings file wrapper using ```Microsoft.Extensions.Configuration``` layout.
* Reflection helpers
* IoC Wrappers implementing ```IDependencyHandle```

## Use with StructureMap
``` CSharp
using CardboardBox.Setup;

namespace SomeApplication
{
	public class Program
	{
		private readonly ISomeService someService;

		public Program(ISomeService someService)
		{
			this.someService = someService;
		}

		public void Start()
		{
			someService.Hello();
		}

		public static void Main(string[] args)
		{
			DependencyInjection.StructureMap()
							   //Chain any CardboardBox utilities like .UseRedis(host)
							   .Build<Program>()
							   .Start();
		}
	}

	public interface ISomeService
	{
		void Hello();
	}

	public class SomeService : ISomeService
	{
		public void Hello()
		{
			Console.WriteLine("Hello world!");
		}
	}
}
```

## Use with Lamar
Extremely similar to StructureMap's layout. Since Lamar is StructureMap's successor, there is no surprise there.

``` CSharp
using CardboardBox.Setup;

namespace SomeApplication
{
	public class Program
	{
		private readonly ISomeService someService;

		public Program(ISomeService someService)
		{
			this.someService = someService;
		}

		public void Start()
		{
			someService.Hello();
		}

		public static void Main(string[] args)
		{
			DependencyInjection.Lamar()
							   //Chain any CardboardBox utilities like .UseRedis(host)
							   .Build<Program>()
							   .Start();
		}
	}

	public interface ISomeService
	{
		void Hello();
	}

	public class SomeService : ISomeService
	{
		public void Hello()
		{
			Console.WriteLine("Hello world!");
		}
	}
}
```

## Use with ServiceCollection layout
A little bit different seeing as we need to register our services with the Service Collection

``` CSharp
using CardboardBox.Setup;

namespace SomeApplication
{
	public class Program
	{
		private readonly ISomeService someService;

		public Program(ISomeService someService)
		{
			this.someService = someService;
		}

		public void Start()
		{
			someService.Hello();
		}

		public static void Main(string[] args)
		{
			DependencyInjection.ServiceCollection()
							   //Chain any CardboardBox utilities like .UseRedis(host)
							   .Use<ISomeService, SomeService>()
							   .Build<Program>()
							   .Start();
		}
	}

	public interface ISomeService
	{
		void Hello();
	}

	public class SomeService : ISomeService
	{
		public void Hello()
		{
			Console.WriteLine("Hello world!");
		}
	}
}
```

## Integration with other services
Due to the way CardboardBox is laid out with being able to support multiple IoC systems, it can easily be integrated with existing sytems as well.

Here is an example of including CardboardBox.Redis into an existing Asp.net core application:
``` CSharp
using CardboardBox.Setup;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace SomeWebApp
{
	public class Startup
	{
		...

		public void ConfigureServices(IServiceCollection services)
		{
			services.CardboardBox()
					.UseRedis("localhost");

			...
		}

		public void Configure()
		{
			...
		}
	}
}
```

## Using settings utility
This was mostly to wrap the standard logic for the IConfiguration and IConfigurationBinder.
There are more options available than the following, this is just the default implementation:

``` CSharp
using CardboardBox.Setup;

namespace SomeApplication
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var settings = Settings.Get<SettingsModel>("appsettings.json");
		}
	}

	public class SettingsModel
	{
		public string SomeFilePath { get; set; }
	}
}
```

Appsettings.json file:
``` JSON
{
	"SomeFilePath": "C:\\Something.cool"
}
```