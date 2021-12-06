# Dependencies:
You'll need to install the following dependencies:

```
Flurl (3.0.2+)
Microsoft.Extensions.Http (5.0.0+)
Newtonsoft.Json (13.0.1)
```

# Implementation:
In your ServiceCollection add the following line:

```csharp
services.AddCardboardHttp();
```

If you want to use Newtonsoft.Json for the serailizer, add the following line instead:

```csharp
services.AddCardboardHttp<NewtonsoftJsonService>();
```

# How to use:

```csharp
public interface ISomeService 
{
	Task<SomeModel> GetModel();
	
	Task<SomeModel> GetCacheModel();
}

public class SomeService : ISomeService
{
	private readonly IApiService _api;
	
	public SomeService(IApiService api)
	{
		_api = api;
	}

	public Task<SomeModel> GetModel()
	{
		return _api.Get<SomeModel>("https://api.example.com/some-model");
	}
	
	public Task<SomeModel> GetCacheModel()
	{
		return _api.CacheGet<SomeModel>("https://api.example.com/some-model");
	}
}

public class SomeModel
{
	public string SomeData { get; set; }
}
```