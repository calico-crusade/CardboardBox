# CardboardBox.Http
Extension to HttpClientFactory that exposes common Http Request methods

## Installation
You can install the nuget package within Visual Studio. It targets .net 6.0 to take advantage of most of the new features within C# and .net.

```
PM> Install-Package CardboardBox.Http
```

## Setup
You can setup the Api Client using the following code:

Where ever you register your services with Dependency Injection, you can add: 
```csharp
using CardboardBox.Http;
using Microsoft.Extensions.DependencyInjection;

...

services.AddCardboardHttp();
```

This will register the IHttpClientFactory as well as all of the other dependencies necessary for handling `CardboardBox.Http`.

## Usage
Once CardboardBox.Http is registered with your service collection you can inject the `IApiService` into any of your services and get access to all of the default methods

```csharp
using CardboardBox.Http;

namespace ExampleHttp
{
  public class SomeService 
  {
	private readonly IApiService _api;

	public SomeService(IApiService api)
	{
	  _api = api;
	}

	public Task<SomeModel> GetSomething()
	{
	  return _api.Get<SomeModel>("https://example.org");
	}
  }
}
```


## Changing the JSON provider
You can change which library is used for JSON Serialization / Deserialization by using one of the overloads available for `CardboardBox.Http`

By default the library uses `System.Text.Json.JsonSerializer` for all serialization methods. However, Newtonsoft.Json is also preinstalled and can be switched out by changing the ServiceCollection registration method to:
```csharp
using CardboardBox.Http;
using CardboardBox.Json;
using Microsoft.Extensions.DependencyInjection;

...
services.AddCardboardHttp<NewtonsoftJsonService>();
```

You can also write your own JSON provider if you need more flexibility. Just ensure that it follows the `IJsonService` interface.

## Caching Provider
There is built in 

## Notes
Other than that, the bot works like any other Discord.Net instance. You can use regular commands with it as well.

This library does not support sub-commands for now. Only top level commands.